using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using NetAgent.Abstractions.Models;

namespace NetAgent.LLM.Caching
{
    public class MemoryLLMResponseCache : ILLMResponseCache
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly LLMCacheOptions _options;

        public MemoryLLMResponseCache(IOptions<LLMCacheOptions> options)
        {
            _options = options.Value;
        }

        public Task<LLMResponse> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.ExpirationTime > DateTime.UtcNow)
                {
                    return Task.FromResult(entry.Response);
                }
                _cache.TryRemove(key, out _);
            }
            return Task.FromResult<LLMResponse>(null);
        }

        public Task SetAsync(string key, LLMResponse response, TimeSpan? expiration = null)
        {
            var expirationTime = DateTime.UtcNow + (expiration ?? _options.DefaultExpiration);
            var entry = new CacheEntry(response, expirationTime);

            // Enforce cache size limit
            while (_cache.Count >= _options.MaxCacheSize)
            {
                var oldestEntry = _cache
                    .OrderBy(x => x.Value.ExpirationTime)
                    .FirstOrDefault();
                
                if (!string.IsNullOrEmpty(oldestEntry.Key))
                {
                    _cache.TryRemove(oldestEntry.Key, out _);
                }
            }

            _cache.AddOrUpdate(key, entry, (_, _) => entry);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.ExpirationTime > DateTime.UtcNow)
                {
                    return Task.FromResult(true);
                }
                _cache.TryRemove(key, out _);
            }
            return Task.FromResult(false);
        }

        public Task RemoveAsync(string key)
        {
            _cache.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        private class CacheEntry
        {
            public LLMResponse Response { get; }
            public DateTime ExpirationTime { get; }

            public CacheEntry(LLMResponse response, DateTime expirationTime)
            {
                Response = response;
                ExpirationTime = expirationTime;
            }
        }
    }
}