using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Projector.DomainEvents
{
    public sealed class UserVerified : DomainEvent
    {
        public UserVerified(string aggregateId) : base(aggregateId) { }
    }
}
