using System.Collections.Generic;

namespace Tacta.EventStore.Domain
{
    public interface IAggregateRoot<out TIdentity> : IEntity<TIdentity> where TIdentity : IEntityId
    {
        int Version { get; }
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    }
}
