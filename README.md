# Tacta.EventStore

[![Build Status](https://app.travis-ci.com/tacta-io/Tacta.EventStore.svg?branch=main)](https://app.travis-ci.com/tacta-io/Tacta.EventStore) [![NuGet Version and Downloads count](https://buildstats.info/nuget/Tacta.EventStore)](https://www.nuget.org/packages/Tacta.EventStore)

 ``` Dapper v2.0.78 ``` ``` Newtonsoft.Json v11.0.2 ``` ``` System.Data.SqlClient v4.8.5 ``` ``` Microsoft.Data.SqlClient v5.0.1 ``` ``` Polly v7.2.2 ``` ``` Microsoft.CSharp v4.7.0 ``` 

 Nuget package for handling domain, event store and projections

Package consist of  
- SQL based EventStore based on dapper and generic repository
- DDD helper classes eg. DomainEvent, AggregateRoot, ValueObjects etc
- Projector package - for handling projection pattern eg. populating read models

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


