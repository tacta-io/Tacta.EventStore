using System.Threading.Tasks;
using Tacta.Connection;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Test.Projector.Repositories
{
    public sealed class UserProjectionRepository : ProjectionRepository
    {
        public UserProjectionRepository(IConnectionFactory connectionFactory, string table) : base(connectionFactory, table)
        {
        }
    }
}
