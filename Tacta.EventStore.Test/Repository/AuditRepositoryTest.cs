using Dapper;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Tacta.EventStore.Repository;
using Xunit;

namespace Tacta.EventStore.Test.Repository
{
    public class AuditRepositoryTest : SqlBaseTest
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

        [Fact]
        public async Task DetectProjectionsGap_DetectsProjectionGap()
        {
            //Given
            await PopulateEventStoreAndAuditLogWithGaps();

            // When
            var result = await _auditRepository.DetectProjectionsGap(0, 10);

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(3);
        }

        [Fact]
        public async Task DetectProjectionsGap_DoesNotDetectGap_WhenSelectedBatchDoesNotHaveAnyGaps()
        {
            //Given
            await PopulateEventStoreAndAuditLogWithGaps();

            // When
            var result = await _auditRepository.DetectProjectionsGap(4, 10);

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(0);
        }

        private async Task PopulateEventStoreAndAuditLogWithGaps()
        {
            using (var connection = ConnectionFactory.Connection())
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync("SET IDENTITY_INSERT [dbo].[EventStore] ON");
                for (long seq = 1; seq <= 20; seq++)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO [dbo].[EventStore] ([Sequence], [Id], [Name], [AggregateId], [Aggregate], [Version], [CreatedAt], [Payload]) " +
                        "VALUES (@Sequence, @Id, @Name, @AggregateId, @Aggregate, @Version, @CreatedAt, @Payload)",
                        new
                        {
                            Id = Guid.NewGuid(),
                            Sequence = seq,
                            Name = $"Event {seq}",
                            AggregateId = $"Event_{seq}",
                            Aggregate = "Event",
                            Version = 1,
                            CreatedAt = DateTime.Now.AddDays(-2),
                            Payload = "{}"
                        });
                }
                await connection.ExecuteAsync("SET IDENTITY_INSERT [dbo].[EventStore] OFF");
            }

            using (var connection = ConnectionFactory.Connection())
            {
                await connection.OpenAsync();
                for (long seq = 1; seq <= 10; seq++)
                {
                    if (seq == 3) continue;
                    await connection.ExecuteAsync(
                        "INSERT INTO [dbo].[ProjectionsAuditLog] ([Sequence], [AppliedAt]) VALUES (@Sequence, @AppliedAt)",
                        new { Sequence = seq, AppliedAt = DateTime.Now.AddMinutes(-30) });
                }
            }
        }
    }
}
