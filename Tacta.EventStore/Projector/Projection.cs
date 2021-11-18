using System.Linq;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Projector
{
    public abstract class Projection : IProjection
    {
        private readonly IProjectionRepository _projectionRepository;
        public int Sequence { get; private set; }

        protected Projection(IProjectionRepository projectionRepository)
        {
            _projectionRepository = projectionRepository;
        }

        public async Task InitializeSequence()
        {
            Sequence = await _projectionRepository.GetSequenceAsync().ConfigureAwait(false);
        }

        public async Task ApplyEvents(int take)
        {
            var events = await _projectionRepository.GetFromSequenceAsync(Sequence, take).ConfigureAwait(false);

            foreach (var @event in events.OrderBy(x => x.Sequence)) await Apply(@event).ConfigureAwait(false);
        }

        public async Task On(IDomainEvent @event)
        {
            await UpdateSequence(@event.Sequence);
        }

        public async Task UpdateSequence(int sequence)
        {
            await Task.FromResult(Sequence = sequence);
        }

        private async Task Apply(IDomainEvent @event)
        {
            await ((dynamic) this).On((dynamic) @event);
        }
    }
}