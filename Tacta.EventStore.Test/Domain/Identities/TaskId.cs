using System;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Domain.Identities
{
    public class TaskId : EntityId
    {
        private readonly Guid _id;

        public TaskId()
        {
            _id = Guid.NewGuid();
        }

        public TaskId(Guid id)
        {
            _id = id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }
    }
}
