# Tacta.EventStore

[![Build Status](https://app.travis-ci.com/tacta-io/Tacta.EventStore.svg?branch=main)](https://app.travis-ci.com/tacta-io/Tacta.EventStore) [![NuGet Version and Downloads count](https://buildstats.info/nuget/Tacta.EventStore)](https://www.nuget.org/packages/Tacta.EventStore)

 ``` Dapper v2.0.143 ``` ``` Newtonsoft.Json v13.0.3 ``` ``` System.Data.SqlClient v4.8.5 ``` ``` Microsoft.Data.SqlClient v5.1.1 ``` ``` Polly v7.2.4 ``` ``` Microsoft.CSharp v4.7.0 ``` 

 ## A simple NuGet package with tactical helpers, event store and projections
 
The package consists of three separate parts that can be used independently of each other:
- ``` Repository ``` SQL event store based on Dapper for loading and storing domain events, handling aggregate versions with optimistic concurrency checks
- ``` Domain ``` Domain-driven design tactical helpers for creating domain events, rehydrating aggregates, comparing value objects, handling entities and their identities
- ``` Projector ``` A simple mechanism for handling projections of domain events and populating read models including retry policies, parallel execution, and optional audit logging and pessimistic processing

 ## Check out [Tactify](https://github.com/tacta-io/Tactify), our example app implemented with Tacta.EventStore

Here are several common questions that can help you get started with developing your new app using Tacta.EventStore:

### How to create the event store?

In the solution, you can find the SQL script [EventStore.sql](https://github.com/tacta-io/Tacta.EventStore/blob/main/EventStore.sql) that can be used to create ``` dbo.EventStore ``` table in the database along with ``` UNIQUE NONCLUSTERED INDEX [ConcurrencyCheckIndex] ON [dbo].[EventStore] ([Version] ASC, [AggregateId] ASC) ``` that won't allow saving two domain events with the same version related to the same aggregate root. Practically, the index is used as an optimistic concurrency check during saving domain events in the event store.

```SQL
 CREATE TABLE [dbo].[EventStore] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [AggregateId] NVARCHAR(100) NOT NULL,
        [Aggregate] NVARCHAR(100) NOT NULL,
        [Version] INT NOT NULL,
        [Sequence] INT IDENTITY(1,1) NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL,
        [Payload] NVARCHAR(MAX) NOT NULL) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

 CREATE UNIQUE NONCLUSTERED INDEX [ConcurrencyCheckIndex] ON [dbo].[EventStore] ([Version] ASC, [AggregateId] ASC) WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = OFF,
        IGNORE_DUP_KEY = OFF,
        DROP_EXISTING = OFF, ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
```


### How to create an aggregate root and execute a command that produces a domain event?

Here is an example of ``` Ticket ``` aggregate root with ``` EstimateTicket ``` command that fires ``` TicketEstimated ``` domain event. You can see some business rules protected there as well as how to rehydrate the aggregate root.

```c#
    public sealed class Ticket : AggregateRoot<TicketId> 
    {
        public override TicketId Id { get; protected set; } // aggregate root identity
        private bool IsClosed { get; set; } = false;
        private bool IsEstimated { get; set; } = false;

        public void EstimateTicket(int numberOfDays, string createdBy) // estimate ticket command
        {
            if (IsClosed) throw new Exception($"Closed ticket {Id} can not be changed.");
            if (numberOfDays < 0) throw new Exception($"Estimation {numberOfDays} in not valid.");

            var @event = new TicketEstimated(Id.ToString(), numberOfDays, createdBy);
            // apply method will add the event to the DomainEvents list
            // update the version of the aggregate root
            // and call On method to rehydrate the aggregate root
            Apply(@event);
        }

        public void On(TicketEstimated _) => IsEstimated = true;
    }
```


### How to save domain events into the event store?

After creating ``` TicketRepository ``` with the ``` SaveAsync ``` method as described below, you can save the changes made on the ``` Ticket ```  by calling ``` await _ticketRepository.SaveAsync(ticket).ConfigureAwait(false) ```.

```c#
     public async Task SaveAsync(Ticket ticket)
     {
         var aggregateRecord = new AggregateRecord(ticket.Id.ToString(), ticket.GetType().Name, ticket.Version);

         var eventRecords = ticket.DomainEvents
             .Select(@event => new EventRecord<IDomainEvent>(((DomainEvent)@event).Id, @event.CreatedAt, @event)).ToList()
             .AsReadOnly();

         await _eventStoreRepository.SaveAsync(aggregateRecord, eventRecords).ConfigureAwait(false);
     }
```


### How to fetch stored domain events and load the aggregate root?

For fetching the domain events from the database, create ``` GetAsync ``` method in the ``` TicketRepository ``` as described below. 

```c#

    public async Task<Ticket> GetAsync(TicketId ticketId)
    {
        var eventStoreRecords = await _eventStoreRepository.GetAsync<DomainEvent>(ticketId.ToString()).ConfigureAwait(false);
   
        eventStoreRecords.ToList().ForEach(x => x.Event.WithVersionAndSequence(x.Version, x.Sequence));
   
        var domainEvents = eventStoreRecords.Select(x => x.Event).ToList().AsReadOnly();
   
        return new Ticket(domainEvents); // constructor will rehydrate the aggregate root
    }      
```

Here is a method that can be part of a domain service for estimating a ticket.
```c#
    public async Task EstimateTicket(TicketId ticketId, int numberOfDays, string createdBy)
    {
        var ticket = await _ticketRepository.GetAsync(ticketId).ConfigureAwait(false);
 
        ticket.EstimateTicket(numberOfDays, createdBy);
 
        await _ticketRepository.SaveAsync(ticket).ConfigureAwait(false);
    }
```

### How to create read models?

When creating a read model, do not forget ``` Sequence ``` property as it is mandatory and it is used to track applied events by projection worker. Here is a simple ``` TicketReadModel ```.

```c#
   public sealed class TicketReadModel
   {
       public long Sequence { get; set; } // required
       public string TicketId { get; set; }
       public string? SprintId { get; set; }
       public string BoardId { get; set; }
       public string Description { get; set; }
       public string? Assignee { get; set; }
       public int? Estimation { get; set; }
       public bool IsClosed { get; set; }
   }
```

After defining a read model, create a table where the model can be saved. Also, create ``` TicketReadModelRepository ``` to be able to do crud operations on the model. Read model repositories need to be inherited from ``` Tacta.EventStore.Repository.ProjectionRepository ```.

### How to create a projection for a read model?

Let's try to create a projection for ``` TicketReadModel ```. First, ``` TicketReadModelProjection ``` needs to be created and inherited from ``` Tacta.EventStore.Projector.Projection ```. Then, for every domain event that needs to be projected to the read model, create ``` On ``` method and define how the domain event will affect the read model.

```c#
   public async Task On(TicketEstimated @event)
   {
       var ticket = new TicketReadModel
       {
           Sequence = @event.Sequence,
           TicketId = @event.AggregateId,
           Estimation = @event.NumberOfDays
       };

       // in the read model repository define how to update the read model in the database
       // with information from the domain event we react to
       await _ticketReadModelRepository.OnTicketEstimatedAsync(ticket).ConfigureAwait(false);
   }
```


### How to run the projections as a background job?

Create a new worker background service and invoke ``` Tacta.EventStore.Projector.ProjectionProcessor ```. 

```c#
   public class Worker : BackgroundService
   {
       private const int PullingInterval = 500; // database pulling rate
       private const int BatchSize = 500; // number of domain events that will be pulled in batch
       private const bool ProcessParallel = true; // if true, projections will be handled in parallel
  
       private readonly IProjectionProcessor _projectionProcessor;
  
       public Worker(IProjectionProcessor projectionProcessor)
       {
           _projectionProcessor = projectionProcessor;
       }
  
       protected override async Task ExecuteAsync(CancellationToken stoppingToken)
       {
           while (!stoppingToken.IsCancellationRequested)
           {
               await _projectionProcessor.Process(BatchSize, ProcessParallel).ConfigureAwait(false);
               await Task.Delay(PullingInterval, stoppingToken);               
           }
       }
   }
```

**Warning!** Having multiple instances of projection worker is not tested yet. We can not guarantee read models will be populated as expected in case multiple instances are deployed. This feature is still in progress.


### How to use pesimistic processing for projections?

The ProjectionProcessor supports a pessimistic processing mode to ensure that event sequences are applied to projections without missing any events. 
When enabled, the processor checks for gaps in the sequence numbers of loaded events. 
If a gap is detected, it will retry loading the events up to 5 times, waiting 1 second between each attempt. 
This helps ensure that all events are processed in strict order.
To enable pessimistic processing, set the `PessimisticProcessing` property to `true` when configuring the `ProjectionProcessor`.

```c#
   await _projectionProcessor.Process(
        batchSize: 500,
        processParallel: true,
        auditEnabled: false,
        pessimisticProcessing: true // enable gap detection and retry
    ).ConfigureAwait(false);

```


### How to use audit logging for projections?

The ProjectionProcessor supports an audit logging feature that tracks which events have been processed by projections. 
When audit is enabled, an entry is written to the audit repository for each processed event, recording the events sequence number and the timestamp when it was applied. 
This provides a reliable way to monitor projection progress, detect missing events, and support troubleshooting or compliance requirements.

To enable audit logging, set the `AuditEnabled` property to `true` when configuring the `ProjectionProcessor`.
```c#
   await _projectionProcessor.Process(
        batchSize: 500,
        processParallel: true,
        auditEnabled: true // enable audit logging
    ).ConfigureAwait(false);
```


### How to use gap detection for projections?

The `DetectProjectionsGap` method allows you to check for missing events in your projections.
It queries the `EventStore` for all sequence numbers in a given range, then left-joins with the `ProjectionsAuditLog` to find which sequences have not been logged as projected.
The result is a list of sequence numbers that are present in the event store but missing from the audit log  these are the events that have not yet been projected.

  - Enable audit logging
  - Detect gaps by calling `DetectProjectionsGap` with the desired sequence range:

``` c#
var gaps = await _projectionProcessor.DetectProjectionsGap(1, 1000).ConfigureAwait(false);
if (gaps.Any())
{
    // Handle gaps, e.g., log them or trigger a reprocessing of the missing events
}
```


### How to auto-heal projections?

The HealProjections class provides an auto-healing mechanism for your projections, allowing you to automatically recover from missed or skipped events in your read models. 
This is especially useful when gaps are detected in the projection audit log, ensuring your projections remain consistent with the event store.      
      
  1. Gap Detection:
     Use the `DetectProjectionsGap` method to find missing event sequences that have not been projected.    
  
  2. Healing Process:
     Pass the list of missing (skipped) sequence numbers to the `HealProjections.TryHeal` method. The healing process works as follows:
        It fetches the aggregate IDs associated with the missing sequences.
        For each affected aggregate, it loads all related events from the event store.
        It optionally loads additional events if needed (the method can be overridden for custom logic).
        For each projection:
            The read model rows associated with the affected aggregate are deleted.
            All events for the aggregate are re-applied using the ForceApply method, which applies events without sequence checks.
            Each event is re-audited by saving its sequence and timestamp to the audit log.

Suppose you have detected gaps in your projections:
    
```c#
    List<long> missingSequences = await _auditRepository.DetectProjectionsGap(1000, 2000);

    if (missingSequences.Any())
    {
        await _healProjections.TryHeal(missingSequences, CancellationToken.None);
    }
```

