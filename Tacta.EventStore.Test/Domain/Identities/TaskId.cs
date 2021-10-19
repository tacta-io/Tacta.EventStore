using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Domain.Identities
{
    public class TaskId : EntityId
    {
        public override string ToString()
        {
            return "task_id";
        }
    }
}
