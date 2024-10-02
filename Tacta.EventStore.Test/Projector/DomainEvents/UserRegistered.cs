using System;
using Newtonsoft.Json;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Projector.DomainEvents
{
    public sealed class UserRegistered : DomainEvent
    {
        public string Name { get; }
        public bool IsBanned { get; }
        public long Sequence { get; }

        public UserRegistered(string aggregateId, string name, bool isBanned, long sequence)
            : base(aggregateId) => (Name, IsBanned, Sequence) = (name, isBanned, sequence);

        [JsonConstructor]
        public UserRegistered(string aggregateId, Guid id, DateTime createdAt, string name, bool isBanned, long sequence)
            : base(id, aggregateId, createdAt) => (Name, IsBanned, Sequence) = (name, isBanned, sequence);
    }
}
