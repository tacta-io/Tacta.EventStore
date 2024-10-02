using System;

namespace Tacta.EventStore.Domain
{
    public interface IDomainEvent
    {
        Guid Id { get; }

        DateTime CreatedAt { get; set; }
    }
}
