using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Repository.Models;

namespace Tacta.EventStore.Repository
{
    public interface IEventStoreRepository
    {
        Task SaveAsync<T>(AggregateRecord aggregateRecord, IReadOnlyCollection<EventRecord<T>> eventRecords, CancellationToken cancellationToken = default);
        Task SaveAsync(IReadOnlyCollection<Aggregate> aggregates, CancellationToken cancellationToken = default);
        Task SaveAsync<T>(T aggregateRoot) where T : IAggregateRoot<IEntityId>;
        Task SaveAsync<T>(IEnumerable<T> aggregateRoots) where T : IAggregateRoot<IEntityId>;

        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetAsync<T>(string aggregateId, CancellationToken cancellationToken = default);
        Task<TA> GetAsync<T, TA>(string aggregateId, CancellationToken cancellationToken = default) where T : DomainEvent where TA : class, IAggregateRoot<IEntityId>;
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetFromSequenceAsync<T>(long sequence, int? take = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetUntilAsync<T>(string aggregateId, Guid eventId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetUntilAsync<T>(string aggregateId, long sequence, CancellationToken cancellationToken = default);
        Task<long> GetLatestSequence(CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetAsync<T>(string query, object param, CancellationToken cancellationToken = default);
    }
}