using System;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Polly;
using Polly.Retry;

namespace Tacta.EventStore.Projector
{
    public class SqlServerResiliencePolicyBuilder
    {
        public int RetryCount { private set; get; }

        public Func<int, TimeSpan> RetrySleepDurationDelegate { private set; get; }

        public SqlServerResiliencePolicyBuilder WithDefaults()
        {
            RetryCount = 5;
            RetrySleepDurationDelegate = retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));

            return this;
        }

        public SqlServerResiliencePolicyBuilder WithRetryCount(int retryCount)
        {
            RetryCount = retryCount;

            return this;
        }

        public SqlServerResiliencePolicyBuilder WithRetrySleepDuration(Func<int, TimeSpan> retrySleepDurationDelegate)
        {
            RetrySleepDurationDelegate = retrySleepDurationDelegate;

            return this;
        }

        public AsyncRetryPolicy BuildTransientErrorRetryPolicy() =>
            Policy
                .Handle<SqlException>(SqlServerTransientExceptionDetector.ShouldRetryOn)
                .Or<TimeoutException>()
                .WaitAndRetryAsync(RetryCount, RetrySleepDurationDelegate);
    }
}