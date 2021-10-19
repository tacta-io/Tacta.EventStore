using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Projector
{
    public interface IProjectionRepository
    {
        Task<int> GetSequence();
        Task<IReadOnlyCollection<IDomainEvent>> GetFromSequenceAsync(int sequence, int take);
    }
}
