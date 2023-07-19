# Tacta.EventStore

[![Build Status](https://app.travis-ci.com/tacta-io/Tacta.EventStore.svg?branch=main)](https://app.travis-ci.com/tacta-io/Tacta.EventStore) [![NuGet Version and Downloads count](https://buildstats.info/nuget/Tacta.EventStore)](https://www.nuget.org/packages/Tacta.EventStore)

 ``` Dapper v2.0.143 ``` ``` Newtonsoft.Json v13.0.3 ``` ``` System.Data.SqlClient v4.8.5 ``` ``` Microsoft.Data.SqlClient v5.1.1 ``` ``` Polly v7.2.4 ``` ``` Microsoft.CSharp v4.7.0 ``` 

 ## A simple NuGet package with tactical helpers, event store and projections
 
The package consists of three separate parts that can be used independently of each other:
- ``` Repository ``` SQL event store based on Dapper for loading and storing domain events, handling aggregate versions with optimistic concurrency checks
- ``` Domain ``` Domain-driven design tactical helpers for creating domain events, rehydrating aggregates, comparing value objects, handling entities and their identities
- ``` Projector ``` A simple mechanism for handling projections of domain events and populating read models including retry policies and parallel execution

 ## Check out [Tactify](https://github.com/tacta-io/Tactify), our example app implemented with Tacta.EventStore

Here are several common questions that can help you get started with developing your new app using Tacta.EventStore:

### How to create an aggregate root and execute a command that produces a domain event?

Here is an example of Ticket aggregate root with EstimateTicket command that fires TicketEstimated domain event. You can see some business rules protected there as well as how to rehydrate the aggregate root.

```c#
    public sealed class Ticket : AggregateRoot<TicketId>
    {
        public override TicketId Id { get; protected set; } 
        private bool IsEstimated { get; set; } = false;

        public void EstimateTicket(int numberOfDays, string createdBy)
        {
            if (IsClosed) throw new Exception($"Closed ticket {Id} can not be changed.");
            if (numberOfDays < 0) throw new Exception($"Estimation {numberOfDays} in not valid.");

            var @event = new TicketEstimated(Id.ToString(), numberOfDays, createdBy);
            Apply(@event);
        }

        public void On(TicketEstimated _) => IsEstimated = true;
    }
```


### How to save domain events into the event store?

Save and rebuild aggregate using event store:

```c#
    var aggregateRecord = new AggregateRecord(booId, "Boo", 0);
    var eventRecords = new List<EventRecord<DomainEvent>>
    {
        new EventRecord<DomainEvent>(booCreated.Id, booCreated.CreatedAt, booCreated)
    };

    //Save aggregate as list of events into event store
    await _eventStoreRepository.SaveAsync(aggregateRecord, eventRecords).ConfigureAwait(false);
    
    //rebuild aggragate using events from eventstore
    var aggregate = await _eventStoreRepository.GetAsync<DomainEvent>(booId).ConfigureAwait(false);
```

