﻿using System;
using Newtonsoft.Json;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Domain.DomainEvents
{
    public class CustomDomainEvent : IDomainEvent
    {
        public Guid Id { get; }
        public long Sequence { get; private set; }
        public int Version { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string CustomDomainEventProperty { get; }

        public CustomDomainEvent(string customDomainEventField)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            Version = 0;
            Sequence = 0;
            CustomDomainEventProperty = customDomainEventField;
        }

        [JsonConstructor]
        public CustomDomainEvent(Guid id, DateTime createdAt, string customDomainEventProperty, int version, long sequence)
        {
            (Id, CreatedAt, CustomDomainEventProperty, Version, Sequence) = (id, createdAt, customDomainEventProperty, version, sequence);
        }
    }
}