using System.Threading.Tasks;
using Tacta.EventStore.Projector;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Test.Projector.DomainEvents;

namespace Tacta.EventStore.Test.Projector.Projections
{
    public class UserProjection : Projection
    {
        public UserProjection(IProjectionRepository projectionRepository) : base(projectionRepository)
        {
        }

        public async Task On(UserRegistered @event)
        {
            await UpdateSequence(@event.Sequence);
        }

        public async Task On(UserBanned @event)
        {
            await UpdateSequence(@event.Sequence);
        }
    }
}