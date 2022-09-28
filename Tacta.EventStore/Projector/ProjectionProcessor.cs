using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly.Retry;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Projector
{
    public class ProjectionProcessor : IProjectionProcessor
    {
        private readonly IEnumerable<IProjection> _projections;
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly AsyncRetryPolicy _retryPolicy;
        private bool _isInitialized;
        private int _pivot;
        

        public ProjectionProcessor(IEnumerable<IProjection> projections, IEventStoreRepository eventStoreRepository)
        {
            _projections = projections;
            _eventStoreRepository = eventStoreRepository;
            _retryPolicy = new SqlServerResiliencePolicyBuilder().WithDefaults().BuildTransientErrorRetryPolicy();
        }

        public async Task<int> Process(int take = 100, bool processParallel = false)
        {
            var processed = 0;

            await _retryPolicy.ExecuteAsync(async () =>
            {
                if (!_isInitialized) await Initialize().ConfigureAwait(false);

                var events = await Load(take).ConfigureAwait(false);

                if (processParallel)
                {
                    var maxDegreeOfParallelism = _projections.Count();
                    Parallel.ForEach(_projections, 
                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = maxDegreeOfParallelism
                        },
                        item =>
                        { 
                            item.Apply(events).ConfigureAwait(false).GetAwaiter().GetResult();
                        });
                }
                else
                {
                    foreach (var projection in _projections)
                    {
                        await projection.Apply(events).ConfigureAwait(false);
                    }
                }
                
                processed = events.Count;

                if (processed > 0) _pivot = events.Max(x => x.Sequence);
            });

            return processed;
        }

        private async Task Initialize()
        {
            foreach (var projection in _projections)
            {
                await projection.Initialize().ConfigureAwait(false);
            }

            _pivot = _projections.Min(x => x.GetSequence());

            _isInitialized = true;
        }

        private async Task<IReadOnlyCollection<IDomainEvent>> Load(int take)
        {
            var eventStoreRecords = await _eventStoreRepository
                .GetFromSequenceAsync<DomainEvent>(_pivot, take).ConfigureAwait(false);

            eventStoreRecords.ToList().ForEach(x => x.Event.WithVersionAndSequence(x.Version, x.Sequence));

            return eventStoreRecords.Select(x => x.Event).ToList().AsReadOnly();
        }
    }
}