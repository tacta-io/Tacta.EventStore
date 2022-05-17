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
            
        }

        public async Task On(UserBanned @event)
        {
            
        }
    }
}