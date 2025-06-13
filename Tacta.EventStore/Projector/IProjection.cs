using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Projector
{
    public interface IProjection
    {
        Task Apply(IReadOnlyCollection<IDomainEvent> events);

        Task Initialize();

        long GetSequence();

        Task Rebuild();
        Task Delete(string aggregateId);
    }
}
