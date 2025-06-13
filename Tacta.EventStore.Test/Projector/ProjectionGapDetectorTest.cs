using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Projector;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Test.Repository;
using Xunit;

namespace Tacta.EventStore.Test.Projector
{
    public class ProjectionGapDetectorTest : SqlBaseTest
    {
        private readonly Mock<IAuditRepository> _auditRepositoryMock;

        public ProjectionGapDetectorTest()
        {
            _auditRepositoryMock = new Mock<IAuditRepository>();
        }

        [Fact]
        public async Task DetectGap_ReturnsSkippedEvents_WhenSkippedProjectionsDetected()
        {
            // Given
            var projectionGapDetector = new ProjectionGapDetector(_auditRepositoryMock.Object);
            long pivot = 100;

            _auditRepositoryMock.Setup(x => x.DetectProjectionsGap(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(new List<long> { 1, 2, 3 });

            // When
            var skippedProjections = await projectionGapDetector.DetectGap(pivot);

            //Then
            Assert.NotNull(skippedProjections);
            Assert.Equal(3, skippedProjections.Count);
        }

        [Fact]
        public async Task DetectGap_ReturnsEmptyList_WhenNoSkippedProjectionsDetected()
        {
            // Given
            var projectionGapDetector = new ProjectionGapDetector(_auditRepositoryMock.Object);
            long pivot = 100;
            _auditRepositoryMock.Setup(x => x.DetectProjectionsGap(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(new List<long>());

            // When
            var skippedProjections = await projectionGapDetector.DetectGap(pivot);

            //Then
            Assert.NotNull(skippedProjections);
            Assert.Empty(skippedProjections);
        }
    }
}
