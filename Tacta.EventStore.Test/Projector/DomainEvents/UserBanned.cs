using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Projector.DomainEvents
{
    public sealed class UserBanned : DomainEvent
    {
        public long Sequence { get; }

        public UserBanned(string aggregateId, long sequence) : base(aggregateId)
        {
            Sequence = sequence;
        }
    }
}
