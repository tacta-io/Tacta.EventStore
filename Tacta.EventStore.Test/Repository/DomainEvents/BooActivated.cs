using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Repository.DomainEvents
{
    public sealed class BooActivated : DomainEvent
    {
        public BooActivated(string aggregateId) : base(aggregateId) { }
    }
}
