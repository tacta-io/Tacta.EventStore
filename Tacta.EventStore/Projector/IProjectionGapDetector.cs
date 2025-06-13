using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tacta.EventStore.Projector
{
    public interface IProjectionGapDetector
    {
        Task<List<long>> DetectGap(long sequenceEnd);
    }
}
