using NetAgent.Abstractions.LLM;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;

namespace NetAgent.LLM.Claude
{
    public class ClaudeLLMProvider : ILLMProvider
    {
        private readonly ClaudeLLMOptions _options;
        private readonly AnthropicClient _client;

        public ClaudeLLMProvider(ClaudeLLMOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _client = new AnthropicClient(_options.ApiKey);
        }

        public string Name => "Claude";

        public async Task<string> GenerateAsync(string prompt, string goal = "", string context = "")
        {
            try
            {
                var message = new MessageParameters
                {
                    Model = _options.Model,
                    MaxTokens = _options.MaxTokens ?? 1000,
                    Temperature = _options.Temperature,
                    Messages = new List<Message>
                    {
                        new Message(RoleType.User,  CombineInputs(prompt,goal,context))
                    }
                };

                var response = await _client.Messages.GetClaudeMessageAsync(message);

                return response.Message.ToString();
            }
            catch (Exception ex)
            {
                throw new LLMException($"Claude API error: {ex.Message}", ex);
            }
        }

        private static string CombineInputs(string prompt, string goal, string context)
        {
            var combined = prompt;

            if (!string.IsNullOrEmpty(goal))
            {
                combined = $"Goal: {goal}\n\n{combined}";
            }

            if (!string.IsNullOrEmpty(context))
            {
                combined = $"Context: {context}\n\n{combined}";
            }

            return combined;
        }
    }
}
