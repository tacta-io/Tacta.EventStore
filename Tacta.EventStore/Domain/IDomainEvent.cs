using System;

namespace Tacta.EventStore.Domain
{
    public interface IDomainEvent
    {
        int Sequence { get; }

        int Version { get; }
        
        DateTime CreatedAt { get; set; }
    }
}
