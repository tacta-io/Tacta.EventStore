using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Test.Projector.Repositories
{
    public sealed class UserProjectionRepository : ProjectionRepository
    {
        public UserProjectionRepository(ISqlConnectionFactory connectionFactory, string table) : base(connectionFactory, table)
        {
        }
    }
}
