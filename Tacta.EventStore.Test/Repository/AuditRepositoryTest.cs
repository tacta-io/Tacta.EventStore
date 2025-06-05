using Dapper;
using System;
using System.Threading.Tasks;
using Tacta.EventStore.Repository;
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
                    "SELECT COUNT(*) FROM ProjectionsAuditLog WHERE Sequence = @Sequence",
                    new { Sequence = 1 });

                Assert.Equal(1, count);
            }
        }

        [Fact]
        public async Task SaveAsync_DoesNotSaveSameSequenceMultipleTimes()
        {
            // When
            await _auditRepository.SaveAsync(1, DateTime.Now.AddDays(-1));
            await _auditRepository.SaveAsync(1, DateTime.Now);
            await _auditRepository.SaveAsync(2, DateTime.Now.AddDays(-1));
            await _auditRepository.SaveAsync(2, DateTime.Now);

            // Then
            using (var connection = ConnectionFactory.Connection())
            {
                var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ProjectionsAuditLog");

                Assert.Equal(2, count);
            }
        }
    }
}
