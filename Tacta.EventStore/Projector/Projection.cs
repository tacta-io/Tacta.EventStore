using System.Linq;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Projector
{
    public abstract class Projection : IProjection
    {
        public int Sequence { get; private set; }

        private readonly IProjectionRepository _projectionRepository;

        protected Projection(IProjectionRepository projectionRepository) => _projectionRepository = projectionRepository;

        public async Task On(IDomainEvent @event) => await UpdateSequence(@event.Sequence);

        public async Task UpdateSequence(int sequence) => await Task.FromResult(Sequence = sequence);

        public async Task InitializeSequence() => Sequence = await _projectionRepository.GetSequence().ConfigureAwait(false);

        public async Task ApplyEvents(int take)
        {
            var events = await _projectionRepository.GetFromSequenceAsync(Sequence, take).ConfigureAwait(false);

            foreach (var @event in events.OrderBy(x => x.Sequence)) await Apply(@event).ConfigureAwait(false);
        }

        private async Task Apply(IDomainEvent @event) => await ((dynamic)this).On((dynamic)@event);
    }
}
