using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Tacta.EventStore.Test.Repository.ReadModels;
using Tacta.EventStore.Test.Repository.Repositories;
using Xunit;

namespace Tacta.EventStore.Test.Repository
{
    public class ProjectionRepositoryTest : SqlBaseTest
    {
        private readonly UserProjectionRepository _projectionRepository;

        public ProjectionRepositoryTest()
        {
            _projectionRepository = new UserProjectionRepository(ConnectionFactory);
        }

        [Fact]
        public async Task GetSequenceAsync_ReturnsMaxSequence()
        {
            // Given
            var models = new List<UserReadModel>
            {
                UserReadModelTestBuilder.Default().WithSequence(10).Build(),
                UserReadModelTestBuilder.Default().WithSequence(123).Build(),
                UserReadModelTestBuilder.Default().WithSequence(5).Build(),
            };

            await _projectionRepository.InsertAsync<UserReadModel>(models);

            // When
            var sequence = await _projectionRepository.GetSequenceAsync();

            // Then
            sequence.Should().Be(123);
        }
    }
}