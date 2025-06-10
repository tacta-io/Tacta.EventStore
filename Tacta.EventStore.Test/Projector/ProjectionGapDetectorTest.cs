using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Projector;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Repository.Exceptions;
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
        public async Task DetectGap_ThrowsProjectionGapException_WhenSkippedProjectionsDetected()
        {
            // Given
            var projectionGapDetector = new ProjectionGapDetector(_auditRepositoryMock.Object);
            long pivot = 100;

            _auditRepositoryMock.Setup(x => x.DetectProjectionsGap(It.IsAny<long>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<long> { 1, 2, 3 });

            // When & Then
            var exception = await Assert.ThrowsAsync<ProjectionGapException>(() => projectionGapDetector.DetectGap(pivot, DateTime.Now.AddHours(-1)));
            Assert.Equal("Skipped projections detected: 1, 2, 3. Please rebuild the projections to ensure consistency.", exception.Message);
        }

        [Fact]
        public async Task DetectGap_DoesNotThrow_WhenNoSkippedProjectionsDetected()
        {
            // Given
            var projectionGapDetector = new ProjectionGapDetector(_auditRepositoryMock.Object);
            long pivot = 100;
            _auditRepositoryMock.Setup(x => x.DetectProjectionsGap(It.IsAny<long>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<long>());

            // When & Then
            await projectionGapDetector.DetectGap(pivot, DateTime.Now.AddHours(-1));
        }
    }
}
