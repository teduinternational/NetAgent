using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.Providers
{
    public class MultiLLMProvider : IMultiLLMProvider 
    {
        private readonly IEnumerable<ILLMProvider> _providers;

        public MultiLLMProvider(IEnumerable<ILLMProvider> providers)
        {
            _providers = providers;
        }

        public string Name => "MultiLLM";

        public async Task<string[]> GenerateFromAllAsync(string prompt)
        {
            var tasks = _providers.Select(p => p.GenerateAsync(prompt));
            return await Task.WhenAll(tasks);
        }

        public async Task<string> GenerateBestAsync(string prompt)
        {
            var results = await GenerateFromAllAsync(prompt);
            return results.OrderByDescending(r => r.Length).FirstOrDefault() ?? string.Empty;
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            return await GenerateBestAsync(prompt);
        }
    }
}
