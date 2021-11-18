using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Test.Repository.Repositories
{
    public class UserProjectionRepository : ProjectionRepository
    {
        public UserProjectionRepository(ISqlConnectionFactory connectionFactory,
            IEventStoreRepository eventStoreRepository) : base(connectionFactory, eventStoreRepository,
            SqlBaseTest.UserReadModelTableName)
        {
        }
    }
}