using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Test.Repository.DomainEvents;
using Tacta.EventStore.Test.Repository.ReadModels;
using Tacta.EventStore.Test.Repository.Repositories;
using Xunit;

namespace Tacta.EventStore.Test.Repository
{
    public class ProjectionRepositoryTest : SqlBaseTest
    {
        private readonly EventStoreRepository _eventStoreRepository;
        private readonly UserProjectionRepository _projectionRepository;

        public ProjectionRepositoryTest()
        {
            _eventStoreRepository = new EventStoreRepository(ConnectionFactory);
            _projectionRepository = new UserProjectionRepository(ConnectionFactory, _eventStoreRepository);
        }

        [Fact]
        public async Task GetSequenceAsync_ReturnsMaxSequence()
        {
            // Given
            var models = CreateUserReadModels();
            await _projectionRepository.InsertAsync<UserReadModel>(models);

            // When
            var sequence = await _projectionRepository.GetSequenceAsync();

            // Then
            sequence.Should().Be(models.Max(m => sequence));
        }

        [Theory]
        [InlineData(0, 50, 3)]
        [InlineData(1, 50, 2)]
        [InlineData(2, 50, 1)]
        [InlineData(1, 2, 2)]
        [InlineData(3, 50, 0)]
        public async Task GetFromSequenceAsync_RangeParams_ReturnsCountBetweenRangeParams(int sequence,
            int take, int expectedCount)
        {
            // Given
            var (aggregateRecord, eventRecords) = CreateFooAggregateWithRegisteredEvents();
            await _eventStoreRepository.SaveAsync(aggregateRecord, eventRecords).ConfigureAwait(false);

            // When
            var results = (await _projectionRepository.GetFromSequenceAsync(sequence, take)).ToList();

            // Then
            results.Count.Should().Be(expectedCount);
        }

        [Fact]
        public async Task GetFromSequenceAsync_NewEventsInserted_ReturnsEventsWithSequenceAndVersion()
        {
            // Given
            var (aggregateRecord, eventRecords) = CreateFooAggregateWithRegisteredEvents();
            await _eventStoreRepository.SaveAsync(aggregateRecord, eventRecords).ConfigureAwait(false);

            // When
            var results = (await _projectionRepository.GetFromSequenceAsync(0, 50)).ToList();

            // Then
            results.Should().NotContain(r => r.Sequence == 0 || r.Version == 0);
        }

        private List<UserReadModel> CreateUserReadModels()
        {
            const int modelCount = 3;

            var models = new List<UserReadModel>();
            for (var i = 1; i <= modelCount; i++)
                models.Add(new UserReadModel
                {
                    Id = Guid.NewGuid(),
                    UpdatedAt = DateTime.Now,
                    Sequence = i,
                    EventId = Guid.NewGuid(),
                    Name = $"test_{i}"
                });

            return models;
        }

        private (AggregateRecord, List<EventRecord<DomainEvent>>) CreateFooAggregateWithRegisteredEvents()
        {
            const int eventCount = 3;

            var fooAggregateRecord = new AggregateRecord($"foo_{Guid.NewGuid()}", "Foo", 0);

            var events = new List<EventRecord<DomainEvent>>();
            for (var i = 0; i < eventCount; i++)
            {
                var fooRegistered = new FooRegistered(fooAggregateRecord.Id, "test_0");

                events.Add(new EventRecord<DomainEvent>(fooRegistered.Id, fooRegistered.CreatedAt, fooRegistered));
            }

            return (fooAggregateRecord, events);
        }
    }
}