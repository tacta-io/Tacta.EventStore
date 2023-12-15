using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tacta.EventStore.Projector
{
    public interface IProjectionProcessor
    {
        Task<int> Process(int take = 100,bool processParallel = false);
        IDictionary<string, int> Status();
    }
}
