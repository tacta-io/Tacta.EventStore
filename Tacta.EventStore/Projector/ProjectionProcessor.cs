using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        private bool _isProcessingPaused;

        public ProjectionProcessor(IEnumerable<IProjection> projections, IEventStoreRepository eventStoreRepository)
        {
            _projections = projections;
            _eventStoreRepository = eventStoreRepository;
            _retryPolicy = new SqlServerResiliencePolicyBuilder().WithDefaults().BuildTransientErrorRetryPolicy();
        }

        public async Task<string> Status(string service, int refreshRate = 5)
        {
            var sequence = await _eventStoreRepository.GetLatestSequence().ConfigureAwait(false);
            var statuses = _projections.ToDictionary(x => x.GetType().Name, x => x.GetSequence());

            var data = new StringBuilder();

            foreach (var status in statuses)
            {
                data.Append($"{{projection:'{status.Key}', sequence:'{status.Value}'}},");
            }

            var content = new StringBuilder(StatusContent.Html)
                .Replace("{refresh}", refreshRate.ToString())
                .Replace("{sequence}", sequence.ToString())
                .Replace("{data}", data.ToString())
                .Replace("{service}", service)
                .ToString();

            return content;
        }

        public async Task<int> Process(int take = 100, bool processParallel = false)
        {
            if (_isProcessingPaused)
            {
                return 0;
            }
            
            var processed = 0;

            await _retryPolicy.ExecuteAsync(async () =>
            {
                if (!_isInitialized) await Initialize().ConfigureAwait(false);

                var events = await Load(take).ConfigureAwait(false);

                try
                {
                    if (processParallel)
                    {
                        var options = new ParallelOptions { MaxDegreeOfParallelism = _projections.Count() };

                        Parallel.ForEach(_projections, options, projection =>
                        {
                            projection.Apply(events).ConfigureAwait(false).GetAwaiter().GetResult();
                        });
                    }
                    else
                    {
                        foreach (var projection in _projections)
                        {
                            await projection.Apply(events).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Process exception {0}", ex);
                    throw;
                }

                processed = events.Count;

                if (processed > 0) _pivot = events.Max(x => x.Sequence);
            });

            return processed;
        }
        
        public async Task Rebuild(IEnumerable<IProjection> projections = null)
        {
            _isProcessingPaused = true;
            
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var projectionsToRebuild = projections ?? _projections;
            
                foreach (var projection in projectionsToRebuild)
                {
                    await projection.Rebuild();
                }

                _pivot = 0;
            });
            
            _isProcessingPaused = false;
        }

        private async Task Initialize()
        {
            foreach (var projection in _projections)
            {
                await projection.Initialize().ConfigureAwait(false);
            }

            _pivot = _projections.Any() ? _projections.Min(x => x.GetSequence()) : 0;

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