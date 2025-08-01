﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Projector
{
    public class HealProjections : IHealProjections
    {
        protected readonly IEventStoreRepository _eventStoreRepository;
        protected readonly IEnumerable<IProjection> _projections;
        protected readonly IAuditRepository _auditRepository;

        public HealProjections(
            IEventStoreRepository eventStoreRepository,
            IEnumerable<IProjection> projections,
            IAuditRepository auditRepository)
        {
            _eventStoreRepository = eventStoreRepository;
            _projections = projections;
            _auditRepository = auditRepository;
        }

        public async Task TryHeal(List<long> skippedSequences, CancellationToken cancellationToken)
        {
            //Fetch distinct aggregate ids by skipped sequences 
            var aggregateIds = await _eventStoreRepository.GetDistinctAggregateIds(skippedSequences, cancellationToken).ConfigureAwait(false);

            var allEvents = new List<EventStoreRecord<DomainEvent>>();

            foreach (var aggregateId in aggregateIds)
            {
                var domainEvents = (await _eventStoreRepository.GetAsync<DomainEvent>(aggregateId, cancellationToken).ConfigureAwait(false)).ToList();
                domainEvents.ForEach(x => x.Event.WithVersionAndSequence(x.Version, x.Sequence));
                domainEvents.AddRange(await LoadAdditionalEvents(aggregateId));
                allEvents.AddRange(domainEvents);
            }

            foreach (var group in allEvents.GroupBy(e => e.AggregateId))
            {
                var aggregateId = group.Key;
                var eventsForAggregate = group
                    .Select(x => (IDomainEvent)x.Event)
                    .OrderBy(e => e.Sequence)
                    .ToList()
                    .AsReadOnly();

                foreach (var projection in _projections)
                {
                    await projection.Delete(aggregateId);
                    await projection.ForceApply(eventsForAggregate);

                    foreach (var @event in eventsForAggregate)
                        await _auditRepository.SaveAsync(@event.Sequence, DateTime.Now);
                }
            }
        }

        public virtual Task<List<EventStoreRecord<DomainEvent>>> LoadAdditionalEvents(string aggregateId)
            => Task.FromResult(new List<EventStoreRecord<DomainEvent>>());
    }
}
