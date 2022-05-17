using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Test.Repository.Repositories
{
    public class UserProjectionRepository : ProjectionRepository
    {
        public UserProjectionRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory, SqlBaseTest.UserReadModelTableName)
        {
        }
    }
}