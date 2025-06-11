using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tacta.EventStore.Repository
{
    /// <summary>
    /// Provides methods for audit logging of projections and detecting gaps in projection application.
    /// </summary>
    public interface IAuditRepository
    {
        /// <summary>
        /// Saves an audit log entry for a projection if it does not already exist.
        /// </summary>
        /// <param name="sequence">The sequence number of the event being projected.</param>
        /// <param name="appliedAt">The timestamp when the projection was applied.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveAsync(long sequence, DateTime appliedAt);

        /// <summary>
        /// Detects gaps in projections by identifying event sequences that have not been applied to projections.
        /// </summary>
        /// <param name="sequenceStart">The starting sequence number to check for gaps.</param>
        /// <param name="sequenceEnd">The ending sequence number to check for gaps.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of event sequence numbers
        /// that are missing from the projections audit log.
        /// </returns>
        Task<List<long>> DetectProjectionsGap(long sequenceStart,long sequenceEnd);
    }
}
