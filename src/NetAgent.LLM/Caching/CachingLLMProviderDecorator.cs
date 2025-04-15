using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.LLM.Monitoring;
using System.Security.Cryptography;
using System.Text;

namespace NetAgent.LLM.Caching
{
    public class CachingLLMProviderDecorator : ILLMProvider
    {
        private readonly ILLMProvider _innerProvider;
        private readonly ILLMResponseCache _cache;
        private readonly ILLMMetricsCollector _metrics;

        public CachingLLMProviderDecorator(ILLMProvider provider, ILLMResponseCache cache, ILLMMetricsCollector metrics)
        {
            _innerProvider = provider;
            _cache = cache;
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

            var cacheKey = GenerateCacheKey(prompt);
            
            // Try to get from cache first
            var cachedResponse = await _cache.GetAsync(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            // Generate new response
            var response = await _innerProvider.GenerateAsync(prompt);
            
            // Cache the response
            await _cache.SetAsync(cacheKey, response);
            
            return response;
        }

        public async Task<bool> IsHealthyAsync()
        {
            try 
            {
                var isProviderHealthy = await _innerProvider.IsHealthyAsync() && await IsCacheHealthyAsync();
                _metrics.RecordHealth(Name, isProviderHealthy); 
                return isProviderHealthy;
            }
            catch (Exception ex)
            {
                _metrics.RecordError(Name, ex.GetType().Name);
                return false;
            }
        }

        private async Task<bool> IsCacheHealthyAsync()
        {
            try
            {
                var testKey = $"health_check_{Guid.NewGuid()}";
                await _cache.SetAsync(testKey, new LLMResponse { Content = "health_check" });
                var result = await _cache.GetAsync(testKey);
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateCacheKey(Prompt prompt)
        {
            var data = Encoding.UTF8.GetBytes(prompt.Content);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return $"{Name}:{Convert.ToBase64String(hash)}";
        }

        public Task<float[]> GetEmbeddingAsync(string input)
        {
            return _innerProvider.GetEmbeddingAsync(input);
        }
    }
}