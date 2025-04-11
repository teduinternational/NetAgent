using NetAgent.Abstractions.LLM;
using System.Threading.Tasks;

namespace NetAgent.LLM.Ollama
{
    public class OllamaProvider : ILLMProvider
    {
        private readonly OllamaOptions _options;

        public OllamaProvider(OllamaOptions options)
        {
            _options = options;
        }

        public string Name => "Ollama";

        public async Task<string> GenerateAsync(string prompt, string goal = "", string context = "")
        {
            // Implementation
            return await Task.FromResult(string.Empty);
        }
    }
}
