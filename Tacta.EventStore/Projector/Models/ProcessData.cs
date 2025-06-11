namespace Tacta.EventStore.Projector.Models
{
    /// <summary>
    /// Represents the result of a projection processing operation,
    /// including the number of processed events and the current pivot sequence.
    /// </summary>
    public sealed class ProcessData
    {
        /// <summary>
        /// Gets or sets the number of events that were processed.
        /// </summary>
        public int Processed { get; set; }

        /// <summary>
        /// Gets or sets the pivot sequence number after processing.
        /// </summary>
        public long Pivot { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessData"/> class.
        /// </summary>
        /// <param name="processed">The number of processed events.</param>
        /// <param name="pivot">The pivot sequence number after processing.</param>
        public ProcessData(int processed, long pivot)
        {
            Processed = processed;
            Pivot = pivot;
        }
    }
}
