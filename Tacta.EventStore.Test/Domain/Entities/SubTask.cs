using Tacta.EventStore.Domain;
using Tacta.EventStore.Test.Domain.Identities;

namespace Tacta.EventStore.Test.Domain.Entities
{
    public sealed class SubTask : Entity<TaskId>
    {
        public override TaskId Id { get; protected set; }

        public string Title { get; private set; }

        public SubTask(TaskId id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
