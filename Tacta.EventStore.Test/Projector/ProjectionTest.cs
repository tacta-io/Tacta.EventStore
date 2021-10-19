using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Projector;
using Tacta.EventStore.Test.Projector.DomainEvents;
using Tacta.EventStore.Test.Projector.Projections;
using Xunit;

namespace Tacta.EventStore.Test.Projector
{
    public class ProjectionTest
    {
        private readonly Mock<IProjectionRepository> _projectionRepository;

        public ProjectionTest()
        {
            _projectionRepository = new Mock<IProjectionRepository>();
        }

        [Fact]
        public async Task OnUserRegistered()
        {
            // Given
            var userRegistered = new UserRegistered("userId", "John Doe", false);
            userRegistered.WithVersionAndSequence(1, 120);

            _projectionRepository.Setup(x => x.GetFromSequenceAsync(0, 100))
                .ReturnsAsync(new List<DomainEvent> { userRegistered });

            // When
            var userProjection = new UserProjection(_projectionRepository.Object);
            var processor = new ProjectionProcessor(new List<IProjection> { userProjection });
            await processor.Process();

            // Then
            Assert.Equal(120, userProjection.Sequence);
        }

        [Fact]
        public async Task OnUserBanned()
        {
            // Given
            var userRegistered = new UserRegistered("userId", "John Doe", false);
            userRegistered.WithVersionAndSequence(1, 120);

            var userBanned = new UserBanned("userId");
            userRegistered.WithVersionAndSequence(2, 345);

            _projectionRepository.Setup(x => x.GetFromSequenceAsync(0, 100))
                .ReturnsAsync(new List<DomainEvent> { userRegistered, userBanned });

            // When
            var userProjection = new UserProjection(_projectionRepository.Object);
            var processor = new ProjectionProcessor(new List<IProjection> { userProjection });
            await processor.Process();

            // Then
            Assert.Equal(345, userProjection.Sequence);
        }
    }
}
