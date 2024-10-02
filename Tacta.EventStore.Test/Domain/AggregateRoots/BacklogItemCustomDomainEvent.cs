using System.Collections.Generic;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Test.Domain.DomainEvents;
using Tacta.EventStore.Test.Domain.Entities;
using Tacta.EventStore.Test.Domain.Identities;

namespace Tacta.EventStore.Test.Domain.AggregateRoots
{
    public class BacklogItemCustomDomainEvent : AggregateRoot<BacklogItemId>
    {
        public override BacklogItemId Id { get; protected set; }

        public string Summary { get; private set; }

        public List<SubTask> SubTasks { get; private set; } = new List<SubTask>();

        private BacklogItemCustomDomainEvent() { }

        public BacklogItemCustomDomainEvent(IReadOnlyCollection<IDomainEvent> events) : base(events)
        {
        }

        public static BacklogItemCustomDomainEvent FromSummary(BacklogItemId id, string summary)
        {
            var item = new BacklogItemCustomDomainEvent();

            item.Apply(new BacklogItemCreated(summary, id));

            return item;
        }

        public void On(BacklogItemCreated @event)
        {
            Id = @event.BacklogItemId;
            Summary = @event.Summary;
        }
    }
}