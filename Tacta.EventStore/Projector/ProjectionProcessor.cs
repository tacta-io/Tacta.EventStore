using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Projector.Models;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Projector
{
    public class ProjectionProcessor : IProjectionProcessor
    {
        private readonly IEnumerable<IProjection> _projections;
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly ILogger<ProjectionProcessor> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private bool _isInitialized;
        private long _pivot;
        private readonly SemaphoreSlim _processingSemaphore = new SemaphoreSlim(1, 1);

        public ProjectionProcessor(
            IEnumerable<IProjection> projections,
            IEventStoreRepository eventStoreRepository,
            IAuditRepository auditRepository,
            ILogger<ProjectionProcessor> logger)
        {
            _projections = projections;
            _eventStoreRepository = eventStoreRepository;
            _retryPolicy = new SqlServerResiliencePolicyBuilder().WithDefaults().BuildTransientErrorRetryPolicy();
            _auditRepository = auditRepository;
            _logger = logger;
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

        public async Task<ProcessData> Process<T>(int take = 100, bool processParallel = false, bool auditEnabled = false, bool pesimisticProcessing = false) where T : IDomainEvent
        {
            var processed = 0;

            await _retryPolicy.ExecuteAsync(async () =>
            {
                if (!_isInitialized)
                    await Initialize().ConfigureAwait(false);

                await _processingSemaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    _logger.LogDebug("Loading {Take} events.", take);
                    var events = await Load<T>(take, pesimisticProcessing).ConfigureAwait(false);

                    if (auditEnabled)
                    {
                        _logger.LogDebug("Audit is enabled. Writing processed events in database.");
                        await AuditEventsAsync(events).ConfigureAwait(false);
                        _logger.LogDebug("Writing processed events in database finished");
                    }

                    if (processParallel)
                    {
                        var options = new ParallelOptions { MaxDegreeOfParallelism = _projections.Count() };

                        Parallel.ForEach(_projections,
                            options,
                            projection => { projection.Apply(events).ConfigureAwait(false).GetAwaiter().GetResult(); });
                    }
                    else
                    {
                        foreach (var projection in _projections)
                            await projection.Apply(events).ConfigureAwait(false);
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

            return new ProcessData(processed, _pivot);
        }

        /// <summary>
        /// Loads events from Event Store as <see cref="DomainEvent"/>
        /// For custom domain events use <see cref="ProjectionProcessor.Process{T}(int, bool, bool, bool)"/>
        /// </summary>
        /// <param name="take">The maximum number of events to process in one batch. Default is 100.</param>
        /// <param name="processParallel">If true, projections are applied in parallel; otherwise, sequentially. Default is false.</param>
        /// <param name="auditEnabled">
        /// If true, an audit entry is written for each processed event.
        /// Enables tracking of event processing for auditing purposes. Default is false.
        /// </param>
        /// <param name="pesimisticProcessing">
        /// If true - loaded events will be checked for gaps in sequence. If gap is detected events will be reloaded 5 times with 1 second delay between loads unless 
        /// event that is missing is loaded.
        /// </param>
        /// <returns>The number of processed events.</returns>
        public async Task<ProcessData> Process(int take = 100, bool processParallel = false, bool auditEnabled = false, bool pessimisticProcessing = false)
            => await Process<DomainEvent>(take, processParallel, auditEnabled, pessimisticProcessing);

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

        private async Task<IReadOnlyCollection<IDomainEvent>> Load<T>(int take, bool pessimisticProcessing = false) where T : IDomainEvent
        {
            IReadOnlyCollection<EventStoreRecord<T>> eventStoreRecords = null;
            bool retry = false;
            int retryCount = 0;

            do
            {
                eventStoreRecords =
                await _eventStoreRepository.GetFromSequenceAsync<T>(
                    _pivot, take).ConfigureAwait(false);
                if (pessimisticProcessing && eventStoreRecords.Any())
                {
                    _logger.LogDebug("Pessimistic processing of event enabled. If gap between sequences is detected delay will occur. Load will be retried with 1 second delay until gape is resolved or limit of 5 retry is reached. After that processing will continue as usual.");
                    var maxSequence = eventStoreRecords.Max(x => x.Sequence);
                    var minSequence = eventStoreRecords.Min(x => x.Sequence);

                    bool hasGap = (maxSequence - minSequence + 1) != eventStoreRecords.Count;

                    if (hasGap && retryCount < 5)
                    {
                        _logger.LogWarning("Retry {retry}: Gap between sequences {MaxSequence} and {MinSequence} detected, expected distance should be {EventsCount}. Loading events will be delayed for 1 second", retryCount + 1, maxSequence, minSequence, eventStoreRecords.Count);
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        retryCount++;
                        retry = true;
                    }
                    else
                    {
                        retry = false;
                    }
                }
            }
            while (retry);

            eventStoreRecords.ToList().ForEach(x => x.Event.WithVersionAndSequence(x.Version, x.Sequence));

            return eventStoreRecords.Select(x => (IDomainEvent)x.Event).ToList().AsReadOnly();
        }

        private async Task AuditEventsAsync(IReadOnlyCollection<IDomainEvent> events)
        {
            foreach (var @event in events)
                await _auditRepository.SaveAsync(@event.Sequence, DateTime.Now).ConfigureAwait(false);
        }
    }
}