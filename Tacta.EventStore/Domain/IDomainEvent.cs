using System;

namespace Tacta.EventStore.Domain
{
    public interface IDomainEvent
    {
        Guid Id { get; }
        
        long Sequence { get; }

        int Version { get; }
        
        DateTime CreatedAt { get; set; }
    }
}
