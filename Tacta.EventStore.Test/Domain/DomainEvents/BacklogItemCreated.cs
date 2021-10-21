using System;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Test.Domain.Identities;

namespace Tacta.EventStore.Test.Domain.DomainEvents
{
    public sealed class BacklogItemCreated : DomainEvent
    {
        public string Summary { get; }

        public BacklogItemId BacklogItemId { get; }

        public BacklogItemCreated(string summary, BacklogItemId backlogItemId) : base(backlogItemId.ToString())
        {
            CreatedAt = DateTime.Now;
            Summary = summary;
            BacklogItemId = backlogItemId;
        }
    }
}
