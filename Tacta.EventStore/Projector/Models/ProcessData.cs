namespace Tacta.EventStore.Projector.Models
{
    public sealed class ProcessData
    {
        public int Processed { get; set; }
        public long Pivot { get; set; }

        public ProcessData(int processed, long pivot)
        {
            Processed = processed;
            Pivot = pivot;
        }
    }
}
