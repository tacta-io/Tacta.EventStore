using System.Threading.Tasks;
using Dapper;

namespace Tacta.EventStore.Repository
{
    public abstract class ProjectionRepository : GenericRepository, IProjectionRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly string _table;

        protected ProjectionRepository(ISqlConnectionFactory connectionFactory, string table) : base(connectionFactory, table)
        {
            _connectionFactory = connectionFactory;
            _table = table;
        }

        public async Task<int> GetSequenceAsync()
        {
            using (var connection = _connectionFactory.SqlConnection())
            {
                var query = $"SELECT MAX (Sequence) FROM {_table}";

                var sequence = await connection.QuerySingleOrDefaultAsync<int?>(query).ConfigureAwait(false);

                return sequence ?? default;
            }
        }
    }
}