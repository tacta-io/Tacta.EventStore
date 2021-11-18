using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Repository
{
    public abstract class ProjectionRepository : GenericRepository, IProjectionRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly string _table;

        protected ProjectionRepository(
            ISqlConnectionFactory connectionFactory,
            IEventStoreRepository eventStoreRepository,
            string table) : base(connectionFactory, table)
        {
            _connectionFactory = connectionFactory;
            _eventStoreRepository = eventStoreRepository;
            _table = table;
        }

        public async Task<int> GetSequenceAsync()
        {
            using (var connection = _connectionFactory.SqlConnection())
            {
                var query = $"SELECT MAX (Sequence) FROM {_table}";

                var sequence = await connection.QuerySingleOrDefaultAsync<int?>(query);

                return sequence ?? default;
            }
        }

        public async Task<IReadOnlyCollection<IDomainEvent>> GetFromSequenceAsync(int sequence, int take)
        {
            var eventStoreRecords = await _eventStoreRepository
                .GetFromSequenceAsync<DomainEvent>(sequence, take).ConfigureAwait(false);

            eventStoreRecords.ToList().ForEach(x => x.Event.WithVersionAndSequence(x.Version, x.Sequence));
            return eventStoreRecords.Select(x => x.Event).ToList().AsReadOnly();
        }
    }
}