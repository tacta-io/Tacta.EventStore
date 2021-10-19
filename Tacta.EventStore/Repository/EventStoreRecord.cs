using System;

namespace Tacta.EventStore.Repository
{
    public sealed class EventStoreRecord<T>
    {
        public string AggregateId { get; set; }
        public int Version { get; set; }
        public int Sequence { get; set; }
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public T Event { get; set; }
    }
}
