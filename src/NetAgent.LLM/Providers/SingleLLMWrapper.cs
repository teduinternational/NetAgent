using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.LLM.Scoring;

namespace NetAgent.LLM.Providers
{
    public class SingleLLMWrapper : IMultiLLMProvider
    {
        private readonly ILLMProvider _provider;

        public SingleLLMWrapper(ILLMProvider provider)
        {
            _provider = provider;
        }

        public string Name => _provider.Name;

        public async Task<LLMResponse[]> GenerateFromAllAsync(Prompt prompt)
        {
            if (!await IsHealthyAsync())
            {
                throw new LLMException($"Provider {Name} is not healthy");
            }

            try
            {
                var response = await _provider.GenerateAsync(prompt);
                return new[] { response };
            }
            catch (TaskCanceledException)
            {
                throw new OperationCanceledException();
            }
        }

        public async Task<LLMResponse> GenerateBestAsync(Prompt prompt)
        {
            if (!await IsHealthyAsync())
            {
                throw new LLMException($"Provider {Name} is not healthy");
            }

            try
            {
                return await _provider.GenerateAsync(prompt);
            }
            catch (TaskCanceledException)
            {
                throw new OperationCanceledException();
            }
        }

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            if (!await IsHealthyAsync())
            {
                throw new LLMException($"Provider {Name} is not healthy");
            }

            try
            {
                return await _provider.GenerateAsync(prompt);
            }
            catch (TaskCanceledException)
            {
                throw new OperationCanceledException();
            }
        }

        public IEnumerable<ILLMProvider> GetProviders()
        {
            return new[] { _provider };
        }

        public IResponseScorer GetScorer()
        {
            return new DefaultResponseScorer();
        }

        public ILogger<IMultiLLMProvider> GetLogger()
        {
            throw new NotImplementedException("Logging is not supported in SingleLLMWrapper");
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                return await _provider.IsHealthyAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<float[]> GetEmbeddingAsync(string input)
        {
            return _provider.GetEmbeddingAsync(input);
        }
    }
}