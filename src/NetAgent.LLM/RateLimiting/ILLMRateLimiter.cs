namespace NetAgent.LLM.RateLimiting
{
    public interface ILLMRateLimiter
    {
        Task WaitAsync(string provider);
        void ReportSuccess(string provider);
        void ReportFailure(string provider);
    }

    public class RateLimitOptions
    {
        public int RequestsPerMinute { get; set; } = 60;
        public int TokensPerMinute { get; set; } = 40000;
        public int ConcurrentRequests { get; set; } = 10;
        public TimeSpan RetryAfter { get; set; } = TimeSpan.FromSeconds(1);
        public bool EnableAdaptiveThrottling { get; set; } = true;
    }

    public class RateLimitExceededException : Exception
    {
        public string Provider { get; }
        public TimeSpan RetryAfter { get; }

        public RateLimitExceededException(string provider, TimeSpan retryAfter)
            : base($"Rate limit exceeded for provider {provider}. Retry after {retryAfter.TotalSeconds} seconds.")
        {
            Provider = provider;
            RetryAfter = retryAfter;
        }
    }
}