using System.Threading.Tasks;

namespace Tacta.EventStore.Projector
{
    public interface IProjection
    {
        Task InitializeSequence();

        Task ApplyEvents(int take);
    }
}
