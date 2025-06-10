using System;
using System.Linq;
using System.Threading.Tasks;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Repository.Exceptions;

namespace Tacta.EventStore.Projector
{
    public class ProjectionGapDetector : IProjectionGapDetector
    {
        private readonly IAuditRepository _auditRepository;
        public ProjectionGapDetector(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task DetectGap(long pivot, DateTime appliedAfter)
        {
            var skipped = await _auditRepository.DetectProjectionsGap(pivot, appliedAfter);

            if (skipped.Any())
            {
                throw new ProjectionGapException(
                    $"Skipped projections detected: {string.Join(", ", skipped)}. " +
                    "Please rebuild the projections to ensure consistency.");
            }
        }
    }
}
