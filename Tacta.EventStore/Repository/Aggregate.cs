using System.Collections.Generic;
using System.Linq;
using Tacta.EventStore.Repository.Exceptions;

namespace Tacta.EventStore.Repository
{
    public sealed class Aggregate<T>
    {
        public AggregateRecord AggregateRecord { get; private set; }
        public IReadOnlyCollection<EventRecord<T>> EventRecords { get; private set; }

        public Aggregate(AggregateRecord aggregateRecord, IReadOnlyCollection<EventRecord<T>> eventRecords)
        {
            if (aggregateRecord == null) throw new InvalidAggregateRecordException("Aggregate record cannot be null");
            if (eventRecords == null) throw new InvalidEventRecordException("Event records cannot be null");
            
            AggregateRecord = aggregateRecord;
            EventRecords = eventRecords;
        }

        internal IEnumerable<StoredEvent> ToStoredEvents()
        {
            var version = AggregateRecord.Version;

            return EventRecords.Select(eventRecord =>
                new StoredEvent
                {
                    AggregateId = AggregateRecord.Id,
                    Aggregate = AggregateRecord.Name,
                    Version = ++version,
                    CreatedAt = eventRecord.CreatedAt,
                    Payload = PayloadSerializer.Serialize(eventRecord.Event),
                    Id = eventRecord.Id,
                    Name = eventRecord.Event.GetType().Name
                });
        }
    }
}