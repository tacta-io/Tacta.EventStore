using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Projector
{
    /// <summary>
    /// Detects gaps in the projection process by verifying that all events within a specified sequence range
    /// have corresponding audit log entries.
    /// 
    /// The <c>checkAuditForSkippedEvents</c> flag is used to reduce the frequency of audit log checks.
    /// It alternates on each call to <see cref="DetectGap(long)"/>, so the audit check is performed
    /// only every other invocation, improving performance by avoiding unnecessary checks.
    /// </summary>
    public class ProjectionGapDetector : IProjectionGapDetector
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ILogger<ProjectionGapDetector> _logger;

        private long _pivot;
        private bool checkAuditForSkippedEvents = false;

        public ProjectionGapDetector(IAuditRepository auditRepository, ILogger<ProjectionGapDetector> logger)
        {
            _auditRepository = auditRepository;
            _pivot = 0;
            _logger = logger;
        }

        /// <summary>
        /// Detects gaps in the projection process by checking for event sequences
        /// that have not been applied to projections between the current pivot and the specified sequence end.
        /// Returns a list of sequence numbers that are missing from the projections audit log.
        /// </summary>
        /// <param name="sequenceEnd">The ending sequence number up to which to check for projection gaps.</param>
        /// <returns>A list of sequence numbers that are missing from the projections audit log.</returns>
        public async Task<List<long>> DetectGap(long sequenceEnd)
        {
            if (_pivot >= sequenceEnd)
            {
                _logger.LogDebug("No gap detection needed: current pivot ({Pivot}) >= sequenceEnd ({SequenceEnd}).", _pivot, sequenceEnd);
                return new List<long>();
            }

            if (checkAuditForSkippedEvents)
            {
                var skipped = await _auditRepository.DetectProjectionsGap(_pivot, sequenceEnd);

                if (skipped.Count > 0)
                {
                    _logger.LogWarning("Detected {Count} missing sequence(s) in projection audit log: {Skipped}.", skipped.Count, string.Join(", ", skipped));
                }
                else
                {
                    _logger.LogDebug("No missing sequences detected in projection audit log between {Pivot} and {SequenceEnd}.", _pivot, sequenceEnd);
                }

                _pivot = sequenceEnd;

                return skipped;
            }

            // skip audit this time, try again on next iteration
            checkAuditForSkippedEvents = !checkAuditForSkippedEvents;

            return new List<long>();
        }
    }
}
