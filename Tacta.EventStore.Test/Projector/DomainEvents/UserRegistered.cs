using System;
using Newtonsoft.Json;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Projector.DomainEvents
{
    public sealed class UserRegistered : DomainEvent
    {
        public string Name { get; }
        public bool IsBanned { get; }

        public UserRegistered(string aggregateId, string name, bool isBanned)
            : base(aggregateId) => (Name, IsBanned) = (name, isBanned);

        [JsonConstructor]
        public UserRegistered(string aggregateId, Guid id, DateTime createdAt, string name, bool isBanned)
            : base(id, aggregateId, createdAt) => (Name, IsBanned) = (name, isBanned);
    }
}
