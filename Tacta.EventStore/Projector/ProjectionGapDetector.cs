using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Repository.Exceptions;

namespace Tacta.EventStore.Projector
{
    /// <summary>
    /// Detects gaps in the projection process by verifying that all events within a specified sequence range
    /// have corresponding audit log entries. Throws an exception if any gaps are found.
    /// </summary>
    public class ProjectionGapDetector : IProjectionGapDetector
    {
        private readonly IAuditRepository _auditRepository;
        private long _pivot;
        private bool checkAuditForSkippedEvents = false;
        public ProjectionGapDetector(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
            _pivot = 0;
        }

        /// <summary>
        /// Detects gaps in the projection process by checking for event sequences
        /// that have not been applied to projections between the current pivot and the specified sequence end.
        /// Throws a <see cref="ProjectionGapException"/> if any gaps are found, indicating skipped projections.
        /// </summary>
        /// <param name="sequenceEnd">The ending sequence number up to which to check for projection gaps.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ProjectionGapException">
        /// Thrown when skipped projections are detected, indicating that some events have not been projected.
        /// </exception>
        public async Task<List<long>> DetectGap(long sequenceEnd)
        {
            if (_pivot >= sequenceEnd)
                return new List<long>();

            if (checkAuditForSkippedEvents)
            {
                var skipped = await _auditRepository.DetectProjectionsGap(_pivot, sequenceEnd);

                _pivot = sequenceEnd;

                return skipped;
            }

            // skip audit this time, try again on next iteration
            checkAuditForSkippedEvents = !checkAuditForSkippedEvents;

            return new List<long>();
        }
    }
}
