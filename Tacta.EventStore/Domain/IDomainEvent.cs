using System;

namespace Tacta.EventStore.Domain
{
    public interface IDomainEvent
    {
        long Sequence { get; }

        int Version { get; }
        
        DateTime CreatedAt { get; set; }
    }
}
