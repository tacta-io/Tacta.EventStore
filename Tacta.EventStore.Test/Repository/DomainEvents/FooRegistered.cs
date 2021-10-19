using System;
using Newtonsoft.Json;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Repository.DomainEvents
{
    public sealed class FooRegistered : DomainEvent
    {
        public string FooDescription { get; }

        public FooRegistered(string aggregateId, string fooDescription)
            : base(aggregateId) => FooDescription = fooDescription;

        [JsonConstructor]
        public FooRegistered(string aggregateId, Guid id, DateTime createdAt, string fooDescription)
            : base(id, aggregateId, createdAt) => FooDescription = fooDescription;
    }
}
