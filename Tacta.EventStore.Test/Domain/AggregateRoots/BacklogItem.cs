using System.Collections.Generic;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Test.Domain.DomainEvents;
using Tacta.EventStore.Test.Domain.Entities;
using Tacta.EventStore.Test.Domain.Identities;

namespace Tacta.EventStore.Test.Domain.AggregateRoots
{
    public sealed class BacklogItem : AggregateRoot<BacklogItemId>
    {
        public override BacklogItemId Id { get; protected set; }

        public string Summary { get; private set; }

        public List<SubTask> SubTasks { get; private set; } = new List<SubTask>();

        private BacklogItem() { }

        public BacklogItem(IReadOnlyCollection<IDomainEvent> events) : base(events)
        {
        }

        public static BacklogItem FromSummary(BacklogItemId id, string summary)
        {
            var item = new BacklogItem();

            item.Apply(new BacklogItemCreated(summary, id));

            return item;
        }

        public TaskId AddTask(string title)
        {
            var task = new SubTask(new TaskId(), title);
            Apply(new SubTaskAdded(task.Id, task.Title));

            return task.Id;
        }

        public void On(BacklogItemCreated @event)
        {
            Id = @event.BacklogItemId;
            Summary = @event.Summary;
        }

        public void On(SubTaskAdded @event)
        {
            var subTask = new SubTask(@event.TaskId, @event.Title);
            SubTasks.Add(subTask);
        }
    }
}
