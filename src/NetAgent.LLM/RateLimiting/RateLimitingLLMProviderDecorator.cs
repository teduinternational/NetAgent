using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;

namespace NetAgent.LLM.RateLimiting
{
    public class RateLimitingLLMProviderDecorator : ILLMProvider
    {
        private readonly ILLMProvider _innerProvider;
        private readonly ILLMRateLimiter _rateLimiter;

        public RateLimitingLLMProviderDecorator(
            ILLMProvider provider,
            ILLMRateLimiter rateLimiter)
        {
            _innerProvider = provider;
            _rateLimiter = rateLimiter;
        }

        public string Name => _innerProvider.Name;

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            if (!await IsHealthyAsync())
            {
                throw new LLMException($"Provider {Name} is not healthy");
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

        public async Task<bool> IsHealthyAsync()
        {
            return await _innerProvider.IsHealthyAsync();
        }
    }
}