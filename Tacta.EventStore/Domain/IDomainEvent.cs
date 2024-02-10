using System;

namespace Tacta.EventStore.Domain
{
    public interface IDomainEvent
    {
        Guid Id { get; }
        
        int Sequence { get; }

        int Version { get; }
        
        DateTime CreatedAt { get; set; }
    }
}
