using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.Providers
{
    public class MultiLLMProvider : IMultiLLMProvider 
    {
        private readonly IEnumerable<ILLMProvider> _providers;
        private readonly IResponseScorer _scorer;

        public MultiLLMProvider(IEnumerable<ILLMProvider> providers, IResponseScorer scorer)
        {
            _providers = providers;
            _scorer = scorer;
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
            return results
                .OrderByDescending(r => _scorer.ScoreResponse(r))
                .FirstOrDefault() ?? string.Empty;
        }

        public async Task<string> GenerateAsync(string prompt, string goal = "", string context = "")
        {
            return await GenerateBestAsync(prompt);
        }
    }
}
