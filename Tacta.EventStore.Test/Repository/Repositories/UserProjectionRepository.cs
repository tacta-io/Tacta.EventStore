using System.Threading.Tasks;
using Tacta.Connection;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Test.Repository.Repositories
{
    public class UserProjectionRepository : ProjectionRepository
    {
        public UserProjectionRepository(IConnectionFactory connectionFactory) : base(connectionFactory, SqlBaseTest.UserReadModelTableName)
        {
        }
    }
}