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
            try 
            {
                var result = await _llm.GenerateAsync(prompt);
                return new[] { result };
            }
            catch (TaskCanceledException)
            {
                throw new OperationCanceledException();
            }
        }

        public async Task<LLMResponse> GenerateBestAsync(Prompt prompt)
        {
            try 
            {
                return await _llm.GenerateAsync(prompt);
            }
            catch (TaskCanceledException)
            {
                throw new OperationCanceledException();
            }
        }

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            try 
            {
                return await _llm.GenerateAsync(prompt);
            }
            catch (TaskCanceledException)
            {
                throw new OperationCanceledException();
            }
        }

        public IEnumerable<ILLMProvider> GetProviders()
        {
            return new[] { _llm };
        }

        public IResponseScorer GetScorer()
        {
            return new DefaultResponseScorer();
        }

        public ILogger<IMultiLLMProvider>? GetLogger()
        {
            return null;
        }
    }
}