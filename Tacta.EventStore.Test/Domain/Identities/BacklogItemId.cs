using System;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Domain.Identities
{
    public class BacklogItemId : EntityId
    {
        private readonly Guid _id;

        public BacklogItemId()
        {
            _id = Guid.NewGuid();
        }

        public BacklogItemId(Guid id)
        {
            _id = id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }
    }
}
