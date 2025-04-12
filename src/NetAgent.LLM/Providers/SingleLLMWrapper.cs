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
        private readonly ILLMProvider _llm;

        public SingleLLMWrapper(ILLMProvider llm)
        {
            _llm = llm;
        }

        public string Name => _llm.Name;

        public async Task<LLMResponse[]> GenerateFromAllAsync(Prompt prompt)
        {
            var result = await _llm.GenerateAsync(prompt);
            return new[] { result };
        }

        public async Task<LLMResponse> GenerateBestAsync(Prompt prompt)
        {
            return await _llm.GenerateAsync(prompt);
        }

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            return await _llm.GenerateAsync(prompt);
        }

        public IEnumerable<ILLMProvider> GetProviders()
        {
            return new[] { _llm };
        }

        public IResponseScorer GetScorer()
        {
            return new DefaultResponseScorer();
        }

        public ILogger<IMultiLLMProvider> GetLogger()
        {
            return null;
        }
    }
}