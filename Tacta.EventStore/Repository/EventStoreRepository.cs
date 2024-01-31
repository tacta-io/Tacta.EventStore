using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Newtonsoft.Json;
using Tacta.Connection;
using Tacta.EventStore.Repository.Exceptions;

namespace Tacta.EventStore.Repository
{
    public sealed class EventStoreRepository : IEventStoreRepository
    {
        private readonly IConnectionFactory _sqlConnectionFactory;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        };

        public EventStoreRepository(IConnectionFactory connectionFactory)
        {
            _sqlConnectionFactory = connectionFactory;
        }

        public EventStoreRepository(IConnectionFactory connectionFactory,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _sqlConnectionFactory = connectionFactory;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public async Task SaveAsync<T>(AggregateRecord aggregateRecord,
            IReadOnlyCollection<EventRecord<T>> eventRecords,
            CancellationToken cancellationToken = default)
        {
            if (eventRecords == null || !eventRecords.Any()) return;

            if (aggregateRecord == null)
                throw new InvalidAggregateRecordException("Aggregate record cannot be null");

            if (eventRecords.Any(x => x == null))
                throw new InvalidEventRecordException("Event record cannot be null");

            var version = aggregateRecord.Version;

            var records = eventRecords.Select(eventRecord => new StoredEvent
            {
                AggregateId = aggregateRecord.Id,
                Aggregate = aggregateRecord.Name,
                Version = ++version,
                CreatedAt = eventRecord.CreatedAt,
                Payload = PayloadSerializer.Serialize(eventRecord.Event),
                Id = eventRecord.Id,
                Name = eventRecord.Event.GetType().Name
            });

            try
            {
                await _sqlConnectionFactory.ExecuteWithTransactionIfExists(async (connection, transaction) =>
                {
                    await connection.ExecuteAsync(StoredEvent.InsertQuery, records, transaction).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if ((e is System.Data.SqlClient.SqlException || e is Microsoft.Data.SqlClient.SqlException) &&
                    e.Message.Contains("ConcurrencyCheckIndex"))
                    throw new ConcurrencyCheckException(e.Message);

                throw;
            }
        }

        public async Task SaveAsync<T>(IReadOnlyCollection<Aggregate<T>> aggregates,
            CancellationToken cancellationToken = default)
        {
            if (aggregates == null || !aggregates.Any()) return;

            var records = aggregates.SelectMany(aggregate => aggregate.ToStoredEvents());

            try
            {
                await _sqlConnectionFactory.ExecuteWithTransactionIfExists(async (connection, transaction) =>
                {
                    await connection.ExecuteAsync(StoredEvent.InsertQuery, records, transaction).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if ((e is System.Data.SqlClient.SqlException || e is Microsoft.Data.SqlClient.SqlException) &&
                    e.Message.Contains("ConcurrencyCheckIndex"))
                    throw new ConcurrencyCheckException(e.Message);

                throw;
            }
        }

        public async Task<IReadOnlyCollection<EventStoreRecord<T>>> GetAsync<T>(string aggregateId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(aggregateId))
                throw new InvalidAggregateIdException("Aggregate Id cannot be null or white space");

            var param = new { AggregateId = aggregateId };

            return await GetAsync<T>(StoredEvent.SelectQuery, param, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<EventStoreRecord<T>>> GetFromSequenceAsync<T>(int sequence,
            int? take = null, CancellationToken cancellationToken = default)
        {
            if (sequence < 0)
                throw new InvalidSequenceException("Sequence cannot be less the zero");

            var query = take.HasValue
                ? StoredEvent.SelectChunkedWithLimitQuery
                : StoredEvent.SelectChunkedWithoutLimitQuery;

            var param = new { Sequence = sequence, Take = take };

            return await GetAsync<T>(query, param, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<EventStoreRecord<T>>> GetUntilAsync<T>(string aggregateId, Guid eventId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(aggregateId))
                throw new InvalidAggregateIdException("Aggregate Id cannot be null or white space");

            if (eventId == Guid.Empty)
                throw new InvalidEventIdException("Event Id cannot be empty");

            var param = new { AggregateId = aggregateId, EventId = eventId };

            return await GetAsync<T>(StoredEvent.SelectUntilEventQuery, param, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<EventStoreRecord<T>>> GetUntilAsync<T>(string aggregateId, int sequence, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(aggregateId))
                throw new InvalidAggregateIdException("Aggregate Id cannot be null or white space");

            if (sequence <= 0)
                throw new InvalidSequenceException("Sequence cannot be zero or less the zero");

            var param = new { AggregateId = aggregateId, Sequence = sequence };

            return await GetAsync<T>(StoredEvent.SelectUntilSequenceQuery, param, cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> GetLatestSequence(CancellationToken cancellationToken = default)
        {
            return await _sqlConnectionFactory.ExecuteWithTransactionIfExists(async (connection, transaction) =>
            {
                return await connection
                    .QueryFirstOrDefaultAsync<int>(StoredEvent.SelectLatestSequenceQuery)
                    .ConfigureAwait(false);

            }, cancellationToken);                
        }

        public async Task<IReadOnlyCollection<EventStoreRecord<T>>> GetAsync<T>(string query, object param, CancellationToken cancellationToken = default)
        {
            return await _sqlConnectionFactory.ExecuteWithTransactionIfExists<IReadOnlyCollection<EventStoreRecord<T>>>(async (connection, transaction) =>
            {
                var storedEvents =
                     (await connection.QueryAsync<StoredEvent>(query, param, transaction).ConfigureAwait(false))
                     .ToList().AsReadOnly();

                if (!storedEvents.Any()) return new List<EventStoreRecord<T>>();

                return storedEvents.Select(@event => new EventStoreRecord<T>
                {
                    Event = PayloadSerializer.Deserialize<T>(@event),
                    AggregateId = @event.AggregateId,
                    CreatedAt = @event.CreatedAt,
                    Id = @event.Id,
                    Version = @event.Version,
                    Sequence = @event.Sequence
                }).OrderBy(x => x.Sequence).ToList().AsReadOnly();
            }, cancellationToken);
        }
    }
}