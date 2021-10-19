using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using Tacta.EventStore.Projector;
using Xunit;

namespace Tacta.EventStore.Test.Projector
{
    public class ProjectionProcessorTest
    {
        private readonly Mock<IProjection> _projectionMock;

        public ProjectionProcessorTest()
        {
            _projectionMock = new Mock<IProjection>();
        }

        [Fact]
        public async Task OnException_ShouldCallInitializeSequenceMethodExactlyOnce()
        {
            // Given
            _projectionMock
                .Setup(x => x.InitializeSequence())
                .Callback(() => throw new Exception());

            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object });

            // When
            var _ = await Record.ExceptionAsync(async () => await processor.Process().ConfigureAwait(false));

            // Then
            _projectionMock.Verify(x => x.InitializeSequence(), Times.Once);
        }

        [Fact]
        public async Task OnTransientSqlException_ShouldCallInitializeSequenceMethodAtLeastTwice()
        {
            // Given
            _projectionMock
                .Setup(x => x.InitializeSequence())
                .Callback(() => throw GenerateRandomTransientSqlException());

            var processor = new ProjectionProcessor(new List<IProjection> { _projectionMock.Object });

            // When
            var _ = await Record.ExceptionAsync(async () => await processor.Process().ConfigureAwait(false));

            // Then
            _projectionMock.Verify(x => x.InitializeSequence(), Times.AtLeast(2));
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
    }
}
