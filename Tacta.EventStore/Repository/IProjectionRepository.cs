using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Repository
{
    public interface IProjectionRepository
    {
        Task<int> GetSequenceAsync();
        Task<IReadOnlyCollection<IDomainEvent>> GetFromSequenceAsync(int sequence, int take);
    }
}