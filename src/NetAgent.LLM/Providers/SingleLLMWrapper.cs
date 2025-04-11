using System.Collections.Generic;
using System.Threading.Tasks;
using NetAgent.Abstractions.LLM;

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

        public async Task<string[]> GenerateFromAllAsync(string prompt)
        {
            var result = await _llm.GenerateAsync(prompt);
            return new[] { result };
        }

        public async Task<string> GenerateBestAsync(string prompt)
        {
            return await _llm.GenerateAsync(prompt);
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            return await _llm.GenerateAsync(prompt);
        }

        public async Task<string> GenerateAsync(string prompt, string systemPrompt, string model)
        {
            return await _llm.GenerateAsync(prompt, systemPrompt, model);
        }
    }
}