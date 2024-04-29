using System;
using Newtonsoft.Json;

namespace Tacta.EventStore.Domain
{
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid Id { get; }

        public string AggregateId { get; }

        [JsonIgnore] public int Version { get; private set; }

        [JsonIgnore] public long Sequence { get; private set; }

        public DateTime CreatedAt { get; set; }

       
        protected DomainEvent(string aggregateId)
        {
            (Id, AggregateId, CreatedAt, Version, Sequence) = (Guid.NewGuid(), aggregateId, DateTime.Now, 0, 0);
        }

        [JsonConstructor]
        protected DomainEvent(Guid id, string aggregateId, DateTime createdAt)
        {
            (Id, AggregateId, CreatedAt) = (id, aggregateId, createdAt);
        }

        public void WithVersionAndSequence(int version, long sequence)
        {
            (Version, Sequence) = (version, sequence);
        }
    }
}
