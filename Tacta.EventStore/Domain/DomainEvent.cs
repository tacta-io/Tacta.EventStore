using System;
using Newtonsoft.Json;

namespace Tacta.EventStore.Domain
{
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid Id { get; }

        public string AggregateId { get; }

        public DateTime CreatedAt { get; set; }

       
        protected DomainEvent(string aggregateId)
        {
            (Id, AggregateId, CreatedAt) = (Guid.NewGuid(), aggregateId, DateTime.Now);
        }

        [JsonConstructor]
        protected DomainEvent(Guid id, string aggregateId, DateTime createdAt)
        {
            (Id, AggregateId, CreatedAt) = (id, aggregateId, createdAt);
        }
    }
}
