using Tacta.Connection;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Test.Repository.Repositories
{
    internal class UserGenericRepository : GenericRepository
    {
        public UserGenericRepository(IConnectionFactory sqlConnectionFactory) : base(sqlConnectionFactory, SqlBaseTest.UserReadModelTableName)
        {
        }
    }
}