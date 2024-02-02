using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Projector
{
    public abstract class Projection : IProjection
    {
        private long _sequence;

        private readonly IProjectionRepository _projectionRepository;

        protected Projection(IProjectionRepository projectionRepository)
        {
            _projectionRepository = projectionRepository;
        }

        public async Task Apply(IReadOnlyCollection<IDomainEvent> events)
        {
            foreach (var @event in events.OrderBy(x => x.Sequence))
            {
                if (@event.Sequence <= _sequence) continue;

                await ((dynamic)this).On((dynamic)@event);

                _sequence = @event.Sequence;
            }
        }

        public async Task Initialize() => _sequence = await _projectionRepository.GetSequenceAsync().ConfigureAwait(false);

        public async Task On(IDomainEvent @event) => await Task.FromResult(_sequence = @event.Sequence).ConfigureAwait(false);

        public long GetSequence() => _sequence;
        
        public async Task Rebuild()
        {
            await _projectionRepository.DeleteAllAsync().ConfigureAwait(false);
            await Initialize().ConfigureAwait(false);
        }
    }
}