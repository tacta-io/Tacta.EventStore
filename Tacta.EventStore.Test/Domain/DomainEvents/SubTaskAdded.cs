using System;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Test.Domain.Identities;

namespace Tacta.EventStore.Test.Domain.DomainEvents
{
    public class SubTaskAdded : DomainEvent
    {
        public IEntityId AggregateId { get; }

        public DateTime CreatedAt { get; set; }

        public string Title { get; }

        public TaskId TaskId { get; }

        public SubTaskAdded(TaskId id, string title) : base(id.ToString())
        {
            CreatedAt = DateTime.Now;
            TaskId = id;
            Title = title;
        }
    }
}
