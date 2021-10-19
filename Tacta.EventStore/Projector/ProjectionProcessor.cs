using System.Collections.Generic;
using System.Threading.Tasks;
using Polly.Retry;

namespace Tacta.EventStore.Projector
{
    public class ProjectionProcessor : IProjectionProcessor
    {
        private const int Take = 100;
        private readonly IEnumerable<IProjection> _projections;
        private readonly AsyncRetryPolicy _retryPolicy;
        private bool _isInitialized;

        public ProjectionProcessor(IEnumerable<IProjection> projections)
        {
            _projections = projections;
            _retryPolicy = new SqlServerResiliencePolicyBuilder()
                .WithDefaults()
                .BuildTransientErrorRetryPolicy();
        }

        public async Task Process()
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                if (!_isInitialized)
                {
                    foreach (var projection in _projections)
                    {
                        await projection.InitializeSequence().ConfigureAwait(false);
                    }

                    _isInitialized = true;
                }

                foreach (var projection in _projections)
                {
                    await projection.ApplyEvents(Take).ConfigureAwait(false);
                }
            });
        }
    }
}