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

        public async Task Apply<T>(IReadOnlyCollection<EventStoreRecord<T>> records)
        {
            foreach (var record in records.OrderBy(x => x.Sequence))
            {
                if (record.Sequence <= _sequence) continue;

                await ((dynamic)this).On((dynamic)record.Event);

                _sequence = record.Sequence;
            }
        }

        public async Task Initialize() => _sequence = await _projectionRepository.GetSequenceAsync().ConfigureAwait(false);

        public long GetSequence() => _sequence;
        
        public async Task Rebuild()
        {
            await _projectionRepository.DeleteAllAsync().ConfigureAwait(false);
            await Initialize().ConfigureAwait(false);
        }
    }
}