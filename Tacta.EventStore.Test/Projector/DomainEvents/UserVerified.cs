using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Projector.DomainEvents
{
    public sealed class UserVerified : DomainEvent
    {
        public long Sequence { get; }

        public UserVerified(string aggregateId, long sequence) : base(aggregateId)
        {
            Sequence = sequence;
        }
    }
}
