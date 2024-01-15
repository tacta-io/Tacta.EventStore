using System.Threading.Tasks;
using Dapper;
using Tacta.Connection;

namespace Tacta.EventStore.Repository
{
    public abstract class ProjectionRepository : GenericRepository, IProjectionRepository
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _table;

        protected ProjectionRepository(IConnectionFactory connectionFactory, string table) : base(connectionFactory, table)
        {
            _connectionFactory = connectionFactory;
            _table = table;
        }

        public async Task<int> GetSequenceAsync()
        {
            using (var connection = _connectionFactory.Connection())
            {
                var query = $"SELECT MAX (Sequence) FROM {_table}";

                var sequence = await connection.QuerySingleOrDefaultAsync<int?>(query).ConfigureAwait(false);

                return sequence ?? default;
            }
        }
    }
}