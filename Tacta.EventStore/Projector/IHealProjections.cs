using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tacta.EventStore.Projector
{
    public interface IHealProjections
    {
        Task TryHeal(List<long> skippedSequences, CancellationToken cancellationToken);
    }
}
