using System;
using System.Threading.Tasks;
using NetAgent.Abstractions.Models;

namespace NetAgent.LLM.Caching
{
    public interface ILLMResponseCache
    {
        Task<LLMResponse> GetAsync(string key);
        Task SetAsync(string key, LLMResponse response, TimeSpan? expiration = null);
        Task<bool> ExistsAsync(string key);
        Task RemoveAsync(string key);
    }

    public class LLMCacheOptions
    {
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(24);
        public int MaxCacheSize { get; set; } = 1000;
        public bool EnableCompression { get; set; } = true;
    }
}