using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using System.Security.Cryptography;
using System.Text;

namespace NetAgent.LLM.Caching
{
    public class CachingLLMProviderDecorator : ILLMProvider
    {
        private readonly ILLMProvider _innerProvider;
        private readonly ILLMResponseCache _cache;

        public CachingLLMProviderDecorator(ILLMProvider provider, ILLMResponseCache cache)
        {
            _innerProvider = provider;
            _cache = cache;
        }

        public string Name => _innerProvider.Name;

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
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

        private string GenerateCacheKey(Prompt prompt)
        {
            var data = Encoding.UTF8.GetBytes(prompt.Content);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return $"{Name}:{Convert.ToBase64String(hash)}";
        }
    }
}