using Microsoft.Extensions.Logging;
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
        private readonly Mock<ILogger<ProjectionGapDetector>> _loggerMock;

        public ProjectionGapDetectorTest()
        {
            _auditRepositoryMock = new Mock<IAuditRepository>();
            _loggerMock = new Mock<ILogger<ProjectionGapDetector>>();
        }

        [Fact]
        public async Task DetectGap_ReturnsSkippedEvents_WhenSkippedProjectionsDetected()
        {
            // Given
            var projectionGapDetector = new ProjectionGapDetector(_auditRepositoryMock.Object, _loggerMock.Object);
            long pivot = 100;

            _auditRepositoryMock.Setup(x => x.DetectProjectionsGap(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(new List<long> { 1, 2, 3 });

            // When
            // First call: toggles the flag, does not check audit
            await projectionGapDetector.DetectGap(pivot);
            // Second call: actually checks audit and returns skipped events
            var skippedProjections = await projectionGapDetector.DetectGap(pivot);

            //Then
            Assert.NotNull(skippedProjections);
            Assert.Equal(3, skippedProjections.Count);
        }

        [Fact]
        public async Task DetectGap_ReturnsEmptyList_WhenNoSkippedProjectionsDetected()
        {
            // Given
            var projectionGapDetector = new ProjectionGapDetector(_auditRepositoryMock.Object, _loggerMock.Object);
            long pivot = 100;
            _auditRepositoryMock.Setup(x => x.DetectProjectionsGap(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(new List<long>());

            // When
            // First call: toggles the flag, does not check audit
            await projectionGapDetector.DetectGap(pivot);
            // Second call: actually checks audit and returns empty list
            var skippedProjections = await projectionGapDetector.DetectGap(pivot);

            //Then
            Assert.NotNull(skippedProjections);
            Assert.Empty(skippedProjections);
        }
    }
}
