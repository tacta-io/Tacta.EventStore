using Dapper;
using System;
using System.Threading.Tasks;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Test.Repository.Repositories;
using Xunit;

namespace Tacta.EventStore.Test.Repository
{
    public class AuditRepositoryTest: SqlBaseTest
    {
        private readonly IAuditRepository _auditRepository;

        public AuditRepositoryTest()
        {
            _auditRepository = new AuditRepository(ConnectionFactory);
        }

        [Fact]
        public async Task SaveAsync_SavesAuditRecord()
        {
            // When
            await _auditRepository.SaveAsync(1, DateTime.Now);

            // Then
            using (var connection = ConnectionFactory.Connection())
            {
                var count = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Audit WHERE Sequence = @Sequence",
                    new { Sequence = 1 });

                Assert.Equal(1, count);
            }
        }
    }
}
