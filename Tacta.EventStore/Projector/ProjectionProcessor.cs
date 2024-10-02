using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
        private long _pivot;
        private readonly SemaphoreSlim _processingSemaphore = new SemaphoreSlim(1, 1);

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

        public async Task<int> Process<T>(int take = 100, bool processParallel = false) where T : IDomainEvent
        {
            var processed = 0;
            
            await _retryPolicy.ExecuteAsync(async () =>
            {
                if (!_isInitialized)
                    await Initialize().ConfigureAwait(false);

                await _processingSemaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    var events = await Load<T>(take).ConfigureAwait(false);
                    
                    if (processParallel)
                    {
                        var options = new ParallelOptions { MaxDegreeOfParallelism = _projections.Count() };

                        Parallel.ForEach(_projections,
                            options,
                            projection => { projection.Apply<T>(events).ConfigureAwait(false).GetAwaiter().GetResult(); });
                    }
                    else
                    {
                        foreach (var projection in _projections)
                            await projection.Apply<T>(events).ConfigureAwait(false);
                    }

                    processed = events.Count;

                    if (processed > 0)
                        _pivot = events.Max(x => x.Sequence);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Process exception {0}", ex);
                    throw;
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            });

            return processed;
        }

        /// <summary>
        /// Loads events from Event Store as <see cref="DomainEvent"/>
        /// For custom domain events use <see cref="ProjectionProcessor.Process{T}(int, bool)"/>
        /// </summary>
        public async Task<int> Process(int take = 100, bool processParallel = false)
        {
            return await Process<DomainEvent>(take, processParallel);
        }

        public async Task Rebuild(IEnumerable<Type> projectionTypes = null)
        {
            await _processingSemaphore.WaitAsync();
            try
            {
                var projectionsToRebuild = new List<IProjection>();
                if (projectionTypes != null)
                    foreach (var projectionType in projectionTypes)
                    {
                        var projection = _projections.FirstOrDefault(x => x.GetType() == projectionType);
                        if (projection != null)
                            projectionsToRebuild.Add(projection);
                    }
                else
                    projectionsToRebuild.AddRange(_projections);

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    foreach (var projection in projectionsToRebuild)
                        await projection.Rebuild();

                    _pivot = 0;
                });
            }
            finally
            {
                _processingSemaphore.Release();
            }
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

        private async Task<IReadOnlyCollection<EventStoreRecord<T>>> Load<T>(int take) where T : IDomainEvent
        {
            var eventStoreRecords = await _eventStoreRepository
                .GetFromSequenceAsync<T>(_pivot, take)
                .ConfigureAwait(false);

            return eventStoreRecords;
        }
    }
}