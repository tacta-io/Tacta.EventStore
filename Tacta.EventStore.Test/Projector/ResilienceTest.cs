using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Projector;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Test.Projector.DomainEvents;
using Tacta.EventStore.Test.Projector.Projections;
using Xunit;

namespace Tacta.EventStore.Test.Projector
{
    public class ResilienceTest
    {
        private IProjection _userProjection;
        private Mock<IEventStoreRepository> _eventStoreRepository;
        private Mock<IProjectionRepository> _userProjectionRepository;

        public ResilienceTest()
        {
            SetupProjectionRepository();
            SetupEventStoreRepository();
        }

        [Fact]
        public async Task OnTransientException_ShouldBeResilient()
        {
            for (var i = 0; i < 10; i++)
            {
                // Given
                _userProjection = new UserProjection(_userProjectionRepository.Object);
                var projections = new List<IProjection> { _userProjection };
                var processor = new ProjectionProcessor(projections, _eventStoreRepository.Object);

                // When
                var numberOfProcessedEvents = await processor.Process().ConfigureAwait(false);

                // Then
                numberOfProcessedEvents.Should().Be(3);
                _userProjection.GetSequence().Should().Be(843);
                (_userProjection as UserProjection).AppliedSequences.Should().BeEquivalentTo(new List<int> { 841, 842, 843 });
            }
        }

        [Fact]
        public async Task WhileRebuildInProgress_ProcessShouldWait()
        {
            // Given
            _userProjection = new VerySlowUserProjection(_userProjectionRepository.Object);
            var projections = new List<IProjection> { _userProjection };
            var processor = new ProjectionProcessor(projections, _eventStoreRepository.Object);

            // When
            var processTask1 = processor.Process().ConfigureAwait(false);
            var rebuildTask = processor.Rebuild().ConfigureAwait(false);
            var processTask2 = processor.Process().ConfigureAwait(false);

            await processTask1;
            await rebuildTask;
            var numberOfProcessedEvents = await processTask2;

            // Then
            numberOfProcessedEvents.Should().Be(3);
            _userProjection.GetSequence().Should().Be(843);
            (_userProjection as VerySlowUserProjection)!.AppliedSequences
                .Should().BeEquivalentTo(new List<int> { 841, 842, 843, 841, 842, 843 });
        }

        public class VerySlowUserProjection : Projection
        {
            public List<long> AppliedSequences { get; }

            public VerySlowUserProjection(IProjectionRepository projectionRepository) : base(projectionRepository)
            {
                AppliedSequences = new List<long>();
            }

            public async Task On(UserRegistered @event)
            {
                AppliedSequences.Add(@event.Sequence);
                await Task.Delay(2000);
            }

            public async Task On(UserBanned @event)
            {
                await Task.Delay(1000);
                AppliedSequences.Add(@event.Sequence);
            }

            public async Task On(UserVerified @event)
            {
                await Task.Delay(3000);
                AppliedSequences.Add(@event.Sequence);
            }
            
        }

        private void SetupProjectionRepository()
        {
            _userProjectionRepository = new Mock<IProjectionRepository>();
            _userProjectionRepository.Setup(x => x.GetSequenceAsync()).ReturnsAsync(0);
        }

        private void SetupEventStoreRepository()
        {
            _eventStoreRepository = new Mock<IEventStoreRepository>();

            var userRegistered = new UserRegistered("User_123", "John Doe", false, 841);

            var userVerified = new UserVerified("User_123", 842);

            var userBanned = new UserBanned("User_123", 843);

            _eventStoreRepository.Setup(x => x.GetFromSequenceAsync<DomainEvent>(0, 100, CancellationToken.None))
                .ReturnsAsync(new List<EventStoreRecord<DomainEvent>>
                {
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userRegistered.AggregateId,
                        CreatedAt = userRegistered.CreatedAt,
                        Event = userRegistered,
                        Id = userRegistered.Id,
                        Sequence = 841,
                        Version = 1
                    },
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userVerified.AggregateId,
                        CreatedAt = userVerified.CreatedAt,
                        Event = userVerified,
                        Id = userVerified.Id,
                        Sequence = 842,
                        Version = 2
                    },
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userBanned.AggregateId,
                        CreatedAt = userBanned.CreatedAt,
                        Event = userBanned,
                        Id = userBanned.Id,
                        Sequence = 843,
                        Version = 3
                    }
                });
        }
    }
}
