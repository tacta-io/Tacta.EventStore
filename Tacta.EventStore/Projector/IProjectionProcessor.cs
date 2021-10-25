using System.Threading.Tasks;

namespace Tacta.EventStore.Projector
{
    public interface IProjectionProcessor
    {
        Task Process();
    }
}
