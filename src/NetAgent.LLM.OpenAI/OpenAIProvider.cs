using NetAgent.Abstractions.LLM;
using System.Threading.Tasks;

namespace NetAgent.LLM.OpenAI
{
    public class OpenAIProvider : ILLMProvider
    {
        private readonly OpenAIOptions _options;

        public OpenAIProvider(OpenAIOptions options)
        {
            _options = options;
        }

        public string Name => "OpenAI";

        public async Task<string> GenerateAsync(string prompt)
        {
            // Implementation
            return await Task.FromResult(string.Empty);
        }
    }
}
