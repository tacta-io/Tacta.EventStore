using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tacta.EventStore.DependencyInjection;
using Tacta.EventStore.Domain;
using Xunit;

namespace Tacta.EventStore.Test.DependencyInjection
{
    #region DuplicateEventForTesting
    public sealed class BacklogItemCreated : DomainEvent
    {
        public BacklogItemCreated(string aggregateId) : base(aggregateId) { }
        public BacklogItemCreated(Guid id, string aggregateId, DateTime createdAt) : base(id, aggregateId, createdAt) { }
    }
    #endregion
    
    public class EventStoreConfigurationTest
    {
        [Fact]
        public void GivenTwoEventsWithTheSameName_WhenRegisteringEventStore_ShouldThrowException()
        {
            // Given
            var services = new ServiceCollection();
            var assemblies = new Assembly[]
            {
                typeof(BacklogItemCreated).Assembly,
            };

            // When + Then
            var exception = Assert.Throws<ArgumentException>(() => services.AddEventStoreRepository(assemblies));
            Assert.Contains(typeof(Domain.DomainEvents.BacklogItemCreated).FullName!, exception.Message);
            Assert.Contains(typeof(Domain.DomainEvents.BacklogItemCreated).Assembly.FullName!, exception.Message);
            Assert.Contains(typeof(BacklogItemCreated).FullName!, exception.Message);
            Assert.Contains(typeof(BacklogItemCreated).Assembly.FullName!, exception.Message);
        }
    }
}