using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Test.Repository.ReadModels;
using Tacta.EventStore.Test.Repository.Repositories;
using Xunit;

namespace Tacta.EventStore.Test.Repository
{
    public class GenericRepositoryTest : SqlBaseTest
    {
        private readonly GenericRepository _genericRepository;

        public GenericRepositoryTest()
        {
            _genericRepository = new UserGenericRepository(ConnectionFactory);
        }

        [Fact]
        public async Task GetAsync_ObjectId_ReturnsObjectWithSameId()
        {
            // Given
            var model = CreateUserReadModel();
            await _genericRepository.InsertAsync(model);

            // When
            var result = await _genericRepository.GetAsync<UserReadModel>(model.Id).ConfigureAwait(false);

            // Then
            result.Should().BeEquivalentTo(model, UseEquivalencyDateTimeOptions());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllObjects()
        {
            // Given
            var models = CreateUserReadModels();
            await _genericRepository.InsertAsync<UserReadModel>(models);

            // When
            var results = (await _genericRepository.GetAllAsync<UserReadModel>().ConfigureAwait(false)).ToList();

            // Then
            results.Should().BeEquivalentTo(models, UseEquivalencyDateTimeOptions());
        }

        [Fact]
        public async Task InsertAsync_SingleObject_InsertsSingleObject()
        {
            // Given
            var model = CreateUserReadModel();

            // When
            await _genericRepository.InsertAsync(model).ConfigureAwait(false);

            // Then
            var result = await _genericRepository.GetAsync<UserReadModel>(model.Id).ConfigureAwait(false);

            result.Should().BeEquivalentTo(model, UseEquivalencyDateTimeOptions());
        }

        [Fact]
        public async Task InsertAsync_MultipleObjects_InsertsMultipleObjects()
        {
            // Given
            var models = CreateUserReadModels();

            // When
            await _genericRepository.InsertAsync<UserReadModel>(models).ConfigureAwait(false);

            // Then
            var results = (await _genericRepository.GetAllAsync<UserReadModel>().ConfigureAwait(false)).ToList();

            results.Should().BeEquivalentTo(models, UseEquivalencyDateTimeOptions());
        }

        [Fact]
        public async Task UpdateAsync_UpdatedObject_UpdatesObject()
        {
            // Given
            var model = CreateUserReadModel();

            await _genericRepository.InsertAsync(model).ConfigureAwait(false);

            var modelForUpdate = await _genericRepository.GetAsync<UserReadModel>(model.Id);
            modelForUpdate.Name = "new_name";

            // When
            await _genericRepository.UpdateAsync(modelForUpdate).ConfigureAwait(false);

            // Then
            var result = await _genericRepository.GetAsync<UserReadModel>(model.Id).ConfigureAwait(false);

            result.Should().BeEquivalentTo(modelForUpdate, UseEquivalencyDateTimeOptions());
        }

        [Fact]
        public async Task DeleteAsync_ObjectId_DeletesObjectWithSameId()
        {
            // Given
            var model = CreateUserReadModel();

            await _genericRepository.InsertAsync(model).ConfigureAwait(false);

            // When
            await _genericRepository.DeleteAsync(model.Id).ConfigureAwait(false);

            // Then
            var result = await _genericRepository.GetAsync<UserReadModel>(model.Id).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAllAsync_DeletesAllObjects()
        {
            // Given
            var models = CreateUserReadModels();

            await _genericRepository.InsertAsync<UserReadModel>(models).ConfigureAwait(false);

            // When
            await _genericRepository.DeleteAllAsync().ConfigureAwait(false);

            // Then
            var result = (await _genericRepository.GetAllAsync<UserReadModel>().ConfigureAwait(false)).ToList();

            result.Should().BeEmpty();
        }

        private UserReadModel CreateUserReadModel()
        {
            var model = new UserReadModel
            {
                Id = Guid.NewGuid(),
                UpdatedAt = DateTime.Now,
                Sequence = 1,
                EventId = Guid.NewGuid(),
                Name = "test"
            };

            return model;
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

        private Func<EquivalencyAssertionOptions<UserReadModel>, EquivalencyAssertionOptions<UserReadModel>>
            UseEquivalencyDateTimeOptions()
        {
            return options => options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
                .WhenTypeIs<DateTime>();
        }
    }
}