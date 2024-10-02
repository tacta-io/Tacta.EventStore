using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Projector
{
    public interface IProjection
    {
        Task Apply<T>(IReadOnlyCollection<EventStoreRecord<T>> events);

        Task Initialize();

        long GetSequence();

        Task Rebuild();
    }
}
