using System;
using System.Threading.Tasks;

namespace Tacta.EventStore.Projector
{
    public interface IProjectionGapDetector
    {
        Task DetectGap(long pivot, DateTime appliedAfter);
    }
}
