using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.LLM.Monitoring;

namespace NetAgent.LLM.RateLimiting
{
    public class RateLimitingLLMProviderDecorator : ILLMProvider
    {
        private readonly ILLMProvider _innerProvider;
        private readonly ILLMRateLimiter _rateLimiter;
        private readonly ILLMMetricsCollector _metrics;

        public RateLimitingLLMProviderDecorator(
            ILLMProvider provider,
            ILLMRateLimiter rateLimiter,
            ILLMMetricsCollector metrics)
        {
            _innerProvider = provider;
            _rateLimiter = rateLimiter;
            _metrics = metrics;
        }

        public string Name => _innerProvider.Name;

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            if (!await IsHealthyAsync())
            {
                _metrics.RecordError(Name, "ProviderUnhealthy");

                return new LLMResponse()
                {
                    Content = "Provider is unhealthy, unable to generate response.",
                    IsError = true,
                };
            }

            try
            {
                await _rateLimiter.WaitAsync(Name);
                var response = await _innerProvider.GenerateAsync(prompt);
                _rateLimiter.ReportSuccess(Name);
                return response;
            }
            catch (Exception ex)
            {
                _rateLimiter.ReportFailure(Name);
                throw;
            }
        }

        public Task<float[]> GetEmbeddingAsync(string input)
        {
            return _innerProvider.GetEmbeddingAsync(input);
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var isProviderHealthy = await _innerProvider.IsHealthyAsync();
                _metrics.RecordHealth(Name, isProviderHealthy);
                _rateLimiter.ReportSuccess(Name);
                return isProviderHealthy;
            }
            catch (Exception ex)
            {
                _rateLimiter.ReportFailure(Name);
                _metrics.RecordError(Name, ex.GetType().Name);
                return false;
            }
        }
    }
}