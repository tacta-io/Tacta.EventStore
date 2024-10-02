using System;
using Newtonsoft.Json;
using Tacta.EventStore.Test.Domain.Identities;

namespace Tacta.EventStore.Test.Domain.DomainEvents
{
    public class BacklogItemCreatedCustomDomainEvent : CustomDomainEvent
    {
        public BacklogItemId BacklogItemId { get; }
        public string Summary { get; }
        
        public BacklogItemCreatedCustomDomainEvent(string customDomainEventField, BacklogItemId backlogItemId, 
            string summary) : base(customDomainEventField)
        {
            BacklogItemId = backlogItemId;
            Summary = summary;
        }

        [JsonConstructor]
        public BacklogItemCreatedCustomDomainEvent(
            Guid id, 
            DateTime createdAt,
            string customDomainEventProperty,
            BacklogItemId backlogItemId,
            string summary
            ) : base(id, createdAt, customDomainEventProperty)
        {
            BacklogItemId = backlogItemId;
            Summary = summary;
        }
    }
}