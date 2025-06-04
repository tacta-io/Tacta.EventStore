using Dapper;
using System;
using System.Threading.Tasks;
using Tacta.Connection;

namespace Tacta.EventStore.Repository
{
    public sealed class AuditRepository : IAuditRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public AuditRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task SaveAsync(long sequence, DateTime appliedAt)
        {
            using(var connection = _connectionFactory.Connection())
            {
                var query = "IF NOT EXISTS (SELECT 1 FROM Audit WHERE Sequence = @Sequence) INSERT INTO Audit (Sequence, AppliedAt) VALUES (@Sequence, @AppliedAt)";
                await connection.ExecuteAsync(query, new { Sequence = sequence, AppliedAt = appliedAt })
                    .ConfigureAwait(false);
            }
        }
    }
}
