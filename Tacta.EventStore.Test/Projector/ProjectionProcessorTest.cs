using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Projector;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Repository.Models;
using Tacta.EventStore.Test.Domain.AggregateRoots;
using Tacta.EventStore.Test.Domain.DomainEvents;
using Tacta.EventStore.Test.Domain.Identities;
using Tacta.EventStore.Test.Repository;
using Tacta.EventStore.Test.Repository.DomainEvents;
using Xunit;

#if USE_SYSTEM_DATA_SQLCLIENT
    using System.Data.SqlClient;
#elif USE_MICROSOFT_DATA_SQLCLIENT
using Microsoft.Data.SqlClient;
#endif

namespace Tacta.EventStore.Test.Projector
{
    public class ProjectionProcessorTest : SqlBaseTest
    {
        private readonly Mock<IProjection> _projectionMock;
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly Mock<IAuditRepository> _auditRepository;

        public ProjectionProcessorTest()
        {
            _projectionMock = new Mock<IProjection>();
            _eventStoreRepository = new EventStoreRepository(ConnectionFactory);
            _auditRepository = new Mock<IAuditRepository>();
        }
        
        [Fact]
        public async Task OnException_ShouldCallInitializeSequenceMethodExactlyOnce()
        {
            // Given
            _projectionMock.Setup(x => x.Initialize()).Callback(() => throw new Exception());
            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            var _ = await Record.ExceptionAsync(async () => await processor.Process(100).ConfigureAwait(false));

            // Then
            _projectionMock.Verify(x => x.Initialize(), Times.Once);
        }

        [Fact]
        public async Task OnTransientSqlException_ShouldCallInitializeSequenceMethodAtLeastTwice()
        {
            // Given
            _projectionMock.Setup(x => x.Initialize()).Callback(() => throw GenerateRandomTransientSqlException());
            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            var _ = await Record.ExceptionAsync(async () => await processor.Process().ConfigureAwait(false));

            // Then
            _projectionMock.Verify(x => x.Initialize(), Times.AtLeast(2));
        }

        [Fact]
        public async Task ProjectionProcessor_ShouldReturnNumberOfProcessedEvents()
        {
            // Given
            var (aggregate, events) = CreateFooAggregateWithRegisteredEvents();
            await _eventStoreRepository.SaveAsync(aggregate, events);
            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            var count = await processor.Process();

            // Then
            count.Processed.Should().Be(3);
        }

        [Fact]
        public async Task Rebuild_ShouldInvokeRebuildOnRepositories()
        {
            // Given
            var (aggregate, events) = CreateFooAggregateWithRegisteredEvents();
            await _eventStoreRepository.SaveAsync(aggregate, events);
            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            await processor.Rebuild();

            // Then
            _projectionMock.Verify(p => p.Rebuild());
        }

        [Fact]
        public async Task Rebuild_OnTransientSqlException_ShouldCallRebuildMethodAtLeastTwice()
        {
            // Given
            _projectionMock.Setup(x => x.Rebuild()).Callback(() => throw GenerateRandomTransientSqlException());
            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            var _ = await Record.ExceptionAsync(async () => await processor.Rebuild().ConfigureAwait(false));

            // Then
            _projectionMock.Verify(x => x.Rebuild(), Times.AtLeast(2));
        }
        
        [Fact]
        public async Task ProjectionProcessor_UsingCustomDomainEvent_ShouldReturnNumberOfProcessedEvents()
        {
            // Given
            var (aggregate, events) = CreateAggregateWithCustomDomainEvent();
            await _eventStoreRepository.SaveAsync(aggregate, events);
            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            var count = await processor.Process<CustomDomainEvent>();

            // Then
            count.Processed.Should().Be(1);
        }

        [Fact]
        public async Task Process_WhenAuditEnabled_CallsSaveAsyncForEachEvent()
        {
            // Given
            var (aggregate, events) = CreateFooAggregateWithRegisteredEvents();
            await _eventStoreRepository.SaveAsync(aggregate, events);

            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            await processor.Process(auditEnabled: true);

            // Then
            _auditRepository.Verify(x => x.SaveAsync(1, It.IsAny<DateTime>()), Times.Once);
            _auditRepository.Verify(x => x.SaveAsync(2, It.IsAny<DateTime>()), Times.Once);
            _auditRepository.Verify(x => x.SaveAsync(3, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task Process_WhenAuditDisabled_DoesNotCallSaveAsync()
        {
            // Given
            var (aggregate, events) = CreateFooAggregateWithRegisteredEvents();
            await _eventStoreRepository.SaveAsync(aggregate, events);

            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            await processor.Process(auditEnabled: false);

            // Assert
            _auditRepository.Verify(x => x.SaveAsync(It.IsAny<long>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task Process_WhenExceptionAfterAudit_CallsSaveAsyncForEachEventBeforeFailure()
        {
            // Given
            var (aggregate, events) = CreateFooAggregateWithRegisteredEvents();
            await _eventStoreRepository.SaveAsync(aggregate, events);

            _projectionMock
                .Setup(x => x.Apply(It.IsAny<IReadOnlyCollection<IDomainEvent>>()))
                .ThrowsAsync(new InvalidOperationException("Apply failed"));

            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            var ex = await Record.ExceptionAsync(() => processor.Process(auditEnabled: true));

            // Then
            ex.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be("Apply failed");
            _auditRepository.Verify(x => x.SaveAsync(1, It.IsAny<DateTime>()), Times.Once);
            _auditRepository.Verify(x => x.SaveAsync(2, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task Process_WhenPesimisticProcessingEnabledWithCreatedAtMoreThan5SecondsAgo_ShouldReturnNumberOfProcessedEvents()
        {
            // Given
            var createdAt = DateTime.Now.AddMinutes(-1);
            var (aggregate, events) = CreateFooAggregateWithRegisteredEventsWithCreatedAt(createdAt);
            await _eventStoreRepository.SaveAsync(aggregate, events);

            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            var count = await processor.Process(auditEnabled: true, pesimisticProcessing: true);

            // Then

            count.Processed.Should().Be(3);
        }

        [Fact]
        public async Task Process_WhenPesimisticProcessingEnabledWithCreatedAtLessThan5SecondsAgo_ShouldReturnNumberZero()
        {
            // Given
            var createdAt = DateTime.Now.AddMinutes(5);
            var (aggregate, events) = CreateFooAggregateWithRegisteredEventsWithCreatedAt(createdAt);
            await _eventStoreRepository.SaveAsync(aggregate, events);

            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            var count = await processor.Process(auditEnabled: true, pesimisticProcessing: true);

            // Then

            count.Processed.Should().Be(0);
        }

        [Fact]
        public async Task Process_ShouldReturnProcessData()
        {
            // Given
            var (aggregate, events) = CreateFooAggregateWithRegisteredEvents();
            await _eventStoreRepository.SaveAsync(aggregate, events);

            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object }, _eventStoreRepository, _auditRepository.Object);

            // When
            var count = await processor.Process();

            // Then

            count.Processed.Should().Be(3);
            count.Pivot.Should().Be(3L);
        }

        private static SqlException GenerateRandomTransientSqlException()
        {
            var random = new Random();
            var errorCodeIndex = random.Next(SqlServerTransientExceptionDetector.TransientSqlErrorCodes.Count);
            var errorCode = SqlServerTransientExceptionDetector.TransientSqlErrorCodes.ElementAt(errorCodeIndex);

            var collectionConstructor = typeof(SqlErrorCollection).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[0],
                null);
            var errorCollection = (SqlErrorCollection)collectionConstructor?.Invoke(null);

            var errorConstructor = typeof(SqlError).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[]
                {
                    typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string),
                    typeof(int), typeof(uint), typeof(Exception)
                }, null);
            var error = errorConstructor?.Invoke(new object[]
                {errorCode, (byte) 0, (byte) 0, "server", "errMsg", "procedure", 100, (uint) 0, null});

            var addMethod = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);
            addMethod?.Invoke(errorCollection, new[] { error });

            var constructor = typeof(SqlException).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid) },
                null);

            var sqlException = (SqlException)constructor?.Invoke(new object[]
                {$"Error message: {errorCode}", errorCollection, new DataException(), Guid.NewGuid()});

            return sqlException;
        }

        private static (AggregateRecord, List<EventRecord<DomainEvent>>) CreateFooAggregateWithRegisteredEvents()
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
        
        private static (AggregateRecord, List<EventRecord<IDomainEvent>>) CreateAggregateWithCustomDomainEvent()
        {
            const int eventCount = 1;

            var backlogItemId = new BacklogItemId();
            var summary = "summary";
            
            var fooAggregateRecord = new AggregateRecord(backlogItemId.ToString(), nameof(BacklogItem), 0);

            var events = new List<EventRecord<IDomainEvent>>();

            for (var i = 0; i < eventCount; i++)
            {
                var backlogItemCreated = new BacklogItemCreatedCustomDomainEvent("customProperty", backlogItemId, summary);

                events.Add(new EventRecord<IDomainEvent>(backlogItemCreated.Id, backlogItemCreated.CreatedAt, backlogItemCreated));
            }

            return (fooAggregateRecord, events);
        }

        private static (AggregateRecord, List<EventRecord<DomainEvent>>) CreateFooAggregateWithRegisteredEventsWithCreatedAt(DateTime createdAt)
        {
            const int eventCount = 3;

            var fooAggregateRecord = new AggregateRecord($"foo_{Guid.NewGuid()}", "Foo", 0);

            var events = new List<EventRecord<DomainEvent>>();

            for (var i = 0; i < eventCount; i++)
            {
                var fooRegistered = new FooRegistered(fooAggregateRecord.Id, Guid.NewGuid(), createdAt, "test_0");

                events.Add(new EventRecord<DomainEvent>(fooRegistered.Id, fooRegistered.CreatedAt, fooRegistered));
            }

            return (fooAggregateRecord, events);
        }
    }
}
