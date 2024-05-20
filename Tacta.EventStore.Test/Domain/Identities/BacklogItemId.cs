using System;
using Newtonsoft.Json;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Domain.Identities
{
    public class BacklogItemId : EntityId
    {
        public Guid Id { get; }

        public BacklogItemId()
        {
            Id = Guid.NewGuid();
        }

        [JsonConstructor]
        public BacklogItemId(Guid id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
