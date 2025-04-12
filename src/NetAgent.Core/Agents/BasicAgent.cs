using NetAgent.Abstractions;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;

namespace NetAgent.Core.Agents
{
    public class BasicAgent : IAgent
    {
        private readonly ILLMProvider _llmProvider;

        public BasicAgent(ILLMProvider llmProvider)
        {
            _llmProvider = llmProvider;
        }

        public async Task<AgentResponse> ProcessAsync(AgentRequest request)
        {
            // Create basic prompt from request
            var prompt = new Prompt($"Goal: {request.Goal}");
            if (request.InputContext.Metadata?.Any() == true)
            {
                prompt.Content += "\nContext:\n" + 
                    string.Join("\n", request.InputContext.Metadata.Select(kv => $"{kv.Key}: {kv.Value}"));
            }

            // Get response from LLM
            var llmResponse = await _llmProvider.GenerateAsync(prompt);

            // Return agent response
            return new AgentResponse 
            {
                Output = llmResponse.Content,
                FinalPrompt = prompt.Content,
                ToolTrace = new(),
                ContinueIteration = false
            };
        }
    }
}
