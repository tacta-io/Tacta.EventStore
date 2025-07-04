﻿using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Projector;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Test.Repository;
using Xunit;
using System.Linq;
using Tacta.EventStore.Test.Projector.DomainEvents;

namespace Tacta.EventStore.Test.Projector
{
    public class HealProjectionsTest : SqlBaseTest
    {
        protected readonly Mock<IEventStoreRepository> _eventStoreRepository;
        protected readonly Mock<IProjection> _projection;
        protected readonly Mock<IAuditRepository> _auditRepository;

        public HealProjectionsTest()
        {
            _eventStoreRepository = new Mock<IEventStoreRepository>();
            _projection = new Mock<IProjection>();
            _auditRepository = new Mock<IAuditRepository>();
        }

        [Fact]
        public async Task TryHeal_HealsProjections_WhenThereAreSkippedEvents()
        {
            // Given
            var skippedSequences = new List<long> { 1, 2, 3 };
            var healProjections = new HealProjections(_eventStoreRepository.Object, new List<IProjection> { _projection.Object }, _auditRepository.Object);
            _eventStoreRepository.Setup(x => x.GetDistinctAggregateIds(skippedSequences, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "aggregate1", "aggregate2" });
            var sampleEvents = new List<EventStoreRecord<DomainEvent>>
            {
                new EventStoreRecord<DomainEvent>
                {
                    AggregateId = "aggregate1",
                    Version = 1,
                    Sequence = 10,
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Event = new Mock<DomainEvent>("aggregate1").Object
                },
                new EventStoreRecord<DomainEvent>
                {
                    AggregateId = "aggregate1",
                    Version = 2,
                    Sequence = 11,
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Event = new Mock<DomainEvent>("aggregate1").Object
                }
            };
            _eventStoreRepository.Setup(x => x.GetAsync<DomainEvent>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sampleEvents);
            _auditRepository.Setup(x => x.SaveAsync(It.IsAny<long>(), It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);

            // When
            await healProjections.TryHeal(skippedSequences, default);

            // Then
            _projection.Verify(x => x.ForceApply(It.IsAny<IReadOnlyList<IDomainEvent>>()), Times.Exactly(1));
            _projection.Verify(x => x.Delete(It.IsAny<string>()), Times.Exactly(1));
            _auditRepository.Verify(x => x.SaveAsync(It.IsAny<long>(), It.IsAny<DateTime>()), Times.Exactly(4));
        }

        [Fact]
        public async Task TryHeal_DoesNotHeal_WhenThereAreNoSkippedEvents()
        {
            // Given
            var skippedSequences = new List<long>();
            var healProjections = new HealProjections(_eventStoreRepository.Object, new List<IProjection> { _projection.Object }, _auditRepository.Object);

            // When
            await healProjections.TryHeal(skippedSequences, default);

            // Then
            _projection.Verify(x => x.ForceApply(It.IsAny<IReadOnlyList<IDomainEvent>>()), Times.Never);
            _projection.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
            _auditRepository.Verify(x => x.SaveAsync(It.IsAny<long>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task TryHeal_ShouldLoadEventsWithCorrectSequence()
        {
            // Given
            var aggregateId = "agg-1";
            var skippedSequences = new List<long> { 1, 2, 3 };

            var eventStoreRepositoryMock = new Mock<IEventStoreRepository>();
            var projectionMock = new Mock<IProjection>();
            var auditRepositoryMock = new Mock<IAuditRepository>();

            eventStoreRepositoryMock
                .Setup(r => r.GetDistinctAggregateIds(skippedSequences, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { aggregateId });

            var domainEvents = new List<EventStoreRecord<DomainEvent>>
            {
                new EventStoreRecord<DomainEvent>
                {
                    AggregateId = aggregateId,
                    Sequence = 42,
                    Event = new UserRegistered(aggregateId, "Name", true)
                }
            };
            eventStoreRepositoryMock
                .Setup(r => r.GetAsync<DomainEvent>(aggregateId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(domainEvents);

            // When
            var healProjections = new HealProjections(
                eventStoreRepositoryMock.Object,
                new List<IProjection> { projectionMock.Object },
                auditRepositoryMock.Object);

            await healProjections.TryHeal(skippedSequences, CancellationToken.None);

            // Then
            projectionMock.Verify(p => p.ForceApply(It.Is<IReadOnlyCollection<IDomainEvent>>(events =>
               events.Any(e => e.Sequence == 42)
            )), Times.Once);
        }

        [Fact]
        public async Task TryHeal_WhenLoadAdditionalEventsReturnsDifferentAggregate_DeletesOnlyMainAggregate_ForceApplyCreatesMultipleRows()
        {
            // Given
            var aggregateId1 = "agg-1";
            var aggregateId2 = "agg-2";
            var skippedSequences = new List<long> { 1 };

            var eventStoreRepositoryMock = new Mock<IEventStoreRepository>();
            var projectionMock = new Mock<IProjection>();
            var auditRepositoryMock = new Mock<IAuditRepository>();

            eventStoreRepositoryMock
                .Setup(r => r.GetDistinctAggregateIds(skippedSequences, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { aggregateId1 });

            var domainEvents = new List<EventStoreRecord<DomainEvent>>
            {
                new EventStoreRecord<DomainEvent>
                {
                    AggregateId = aggregateId1,
                    Sequence = 1,
                    Version = 1,
                    Event = new TestDomainEvent(aggregateId1)
                }
            };
            eventStoreRepositoryMock
                .Setup(r => r.GetAsync<DomainEvent>(aggregateId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(domainEvents);

            var additionalEvents = new List<EventStoreRecord<DomainEvent>>
            {
                new EventStoreRecord<DomainEvent>
                {
                    AggregateId = aggregateId2,
                    Sequence = 2,
                    Version = 1,
                    Event = new TestDomainEvent(aggregateId2)
                }
            };

            var healProjections = new TestHealProjections(
                eventStoreRepositoryMock.Object,
                new List<IProjection> { projectionMock.Object },
                auditRepositoryMock.Object,
                additionalEvents
            );

            // When
            await healProjections.TryHeal(skippedSequences, CancellationToken.None);

            // Then
            projectionMock.Verify(p => p.Delete(aggregateId1), Times.Once);
            projectionMock.Verify(p => p.Delete(aggregateId2), Times.Once);

            projectionMock.Verify(p => p.ForceApply(It.Is<IReadOnlyCollection<IDomainEvent>>(events =>
            events.Any(e => e is TestDomainEvent && ((TestDomainEvent)e).AggregateId == aggregateId1) &&
            events.Count == 1
        )), Times.Once);
        }

        private class TestDomainEvent : DomainEvent
        {
            public TestDomainEvent(string aggregateId) : base(aggregateId) { }
        }

        private class TestHealProjections : HealProjections
        {
            private readonly List<EventStoreRecord<DomainEvent>> _additionalEvents;
            public TestHealProjections(
                IEventStoreRepository eventStoreRepository,
                IEnumerable<IProjection> projections,
                IAuditRepository auditRepository,
                List<EventStoreRecord<DomainEvent>> additionalEvents)
                : base(eventStoreRepository, projections, auditRepository)
            {
                _additionalEvents = additionalEvents;
            }

            public override Task<List<EventStoreRecord<DomainEvent>>> LoadAdditionalEvents(string aggregateId)
                => Task.FromResult(_additionalEvents);
        }
    }
}
