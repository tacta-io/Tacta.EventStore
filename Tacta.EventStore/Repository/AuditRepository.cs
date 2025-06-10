using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
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
                var query = "IF NOT EXISTS (SELECT 1 FROM ProjectionsAuditLog WHERE Sequence = @Sequence) INSERT INTO ProjectionsAuditLog (Sequence, AppliedAt) VALUES (@Sequence, @AppliedAt)";
                await connection.ExecuteAsync(query, new { Sequence = sequence, AppliedAt = appliedAt })
                    .ConfigureAwait(false);
            }
        }

        public async Task<List<long>> DetectProjectionsGap(long pivot, DateTime appliedAfter)
        {
            using (var connection = _connectionFactory.Connection())
            {
                var sql = $@"SELECT ES.Sequence 
                          FROM dbo.EventStore ES 
                          LEFT JOIN dbo.ProjectionsAuditLog PA ON ES.Sequence = PA.Sequence
                          WHERE PA.Sequence IS NULL
                          AND ES.Sequence <= @Pivot
                          ORDER BY ES.Sequence";

                var result = await connection.QueryAsync<long>(sql, new { Pivot = pivot }).ConfigureAwait(false);
                return result.ToList();
            }
        }
    }
}
