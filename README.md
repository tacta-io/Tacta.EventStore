# Tacta.EventStore

[![Build Status](https://app.travis-ci.com/tacta-io/Tacta.EventStore.svg?branch=main)](https://app.travis-ci.com/tacta-io/Tacta.EventStore) [![NuGet Version and Downloads count](https://buildstats.info/nuget/Tacta.EventStore)](https://www.nuget.org/packages/Tacta.EventStore)

 ``` Dapper v2.0.143 ``` ``` Newtonsoft.Json v13.0.3 ``` ``` System.Data.SqlClient v4.8.5 ``` ``` Microsoft.Data.SqlClient v5.1.1 ``` ``` Polly v7.2.4 ``` ``` Microsoft.CSharp v4.7.0 ``` 

 A simple NuGet package with ``` tactical helpers ```, ``` event store ``` and ``` projections ```.
 Check out our example app implemented with ``` Tacta.EventStore ``` ``` https://github.com/tacta-io/Tactify ```.

The package consists of three separate parts that can be used independently of each other:
``` Repository ``` SQL-based EventStore based on Dapper for loading and storing domain events, handling aggregate versions and concurrency checks.
``` Domain ``` DDD tactical helpers for DomainEvents, AggregateRoots, ValueObjects, Entities, Identities
``` Projector ``` Mechanism for handling projections of domain events and populating read models

Quick example:

### Event Store:

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


