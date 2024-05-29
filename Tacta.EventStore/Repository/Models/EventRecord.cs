using System;
using Tacta.EventStore.Repository.Exceptions;

namespace Tacta.EventStore.Repository.Models
{
    public sealed class EventRecord<T>
    {
        public Guid Id { get; }
        public DateTime CreatedAt { get; }
        public T Event { get; }

        public EventRecord(Guid id, DateTime createdAt, T @event)
        {
            if (id == Guid.Empty) throw new InvalidEventIdException("Id needs to be set");

            if (createdAt == default) throw new InvalidEventRecordException("CreatedAt needs to be set");

            if (@event == null) throw new InvalidEventRecordException("Event cannot be null");

            (Id, CreatedAt, Event) = (id, createdAt, @event);
        }
    }
}
