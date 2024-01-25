using System.Threading.Tasks;

namespace Tacta.EventStore.Repository
{
    public interface IProjectionRepository
    {
        Task<long> GetSequenceAsync();
    }
}