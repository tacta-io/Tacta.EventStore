using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Projector;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Test.Projector.DomainEvents;

namespace Tacta.EventStore.Test.Projector.Projections
{
    public class UserProjection : Projection
    {
        public List<int> AppliedSequences { get; }

        public UserProjection(IProjectionRepository projectionRepository) : base(projectionRepository)
        {
            AppliedSequences = new List<int>();
        }

        public async Task On(UserRegistered @event)
        {
            AppliedSequences.Add(@event.Sequence);
        }

        public async Task On(UserBanned @event)
        {
            AppliedSequences.Add(@event.Sequence);
        }

        public async Task On(UserVerified @event)
        {
            if (DateTime.Now.Second % 3 == 0) throw new TimeoutException();

            AppliedSequences.Add(@event.Sequence);
        }
    }
}