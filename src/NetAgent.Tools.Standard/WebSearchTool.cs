using NetAgent.Abstractions.Tools;

namespace NetAgent.Tools.Standard
{
    public class WebSearchTool : IAgentTool
    {
        public string Name => "websearch";

        public Task<string> ExecuteAsync(string input)
        {
            // TODO: Replace with real web search API call (e.g., Bing, Google)
            return Task.FromResult($"[Simulated search result for: '{input}']");
        }
    }
}
