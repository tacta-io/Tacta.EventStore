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
        private long _pivot;
        public ProjectionGapDetector(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
            _pivot = 0;
        }

        public async Task DetectGap(long sequenceEnd)
        {
            if (_pivot >= sequenceEnd)
                return;

            var skipped = await _auditRepository.DetectProjectionsGap(_pivot, sequenceEnd);

            if (skipped.Any())
            {
                throw new ProjectionGapException(
                    $"Skipped projections detected: {string.Join(", ", skipped)}. " +
                    "Please rebuild the projections to ensure consistency.");
            }

            _pivot = sequenceEnd;
        }
    }
}
