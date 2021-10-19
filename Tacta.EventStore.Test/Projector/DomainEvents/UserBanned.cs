using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Projector.DomainEvents
{
    public sealed class UserBanned : DomainEvent
    {
        public UserBanned(string aggregateId) : base(aggregateId) { }
    }
}
