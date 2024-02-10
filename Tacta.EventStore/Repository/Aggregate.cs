using System.Collections.Generic;
using System.Linq;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Repository.Exceptions;

namespace Tacta.EventStore.Repository
{
    public sealed class Aggregate
    {
        public AggregateRecord AggregateRecord { get; }
        public IReadOnlyCollection<EventRecord<IDomainEvent>> EventRecords { get; }

        public Aggregate(AggregateRecord aggregateRecord, IReadOnlyCollection<EventRecord<IDomainEvent>> eventRecords)
        {
            if (aggregateRecord == null) throw new InvalidAggregateRecordException("Aggregate record cannot be null");
            if (eventRecords == null) throw new InvalidEventRecordException("Event records cannot be null");
            
            AggregateRecord = aggregateRecord;
            EventRecords = eventRecords;
        }

        public Aggregate(IAggregateRoot<IEntityId> aggregateRoot)
        {
            var aggregateRecord = new AggregateRecord(
                aggregateRoot.Id.ToString(),
                aggregateRoot.GetType().Name,
                aggregateRoot.Version);
            var eventRecords = aggregateRoot.DomainEvents
                .Select(@event => new EventRecord<IDomainEvent>(
                                        @event.Id,
                                        @event.CreatedAt,
                                        @event))
                .ToList()
                .AsReadOnly();

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