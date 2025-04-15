using NetAgent.Abstractions.LLM;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using NetAgent.Abstractions.Models;

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

        public string Name => "claude";

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
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
                        new Message(RoleType.User, prompt.Content)
                    }
                };

                var response = await _client.Messages.GetClaudeMessageAsync(message);

                return new LLMResponse()
                {
                    Content = response.FirstMessage.ToString(),
                    ModelName = response.Model,
                    TokensUsed = response.Usage.OutputTokens,
                };
            }
            catch (Exception ex)
            {
                throw new LLMException($"Claude API error: {ex.Message}", ex);
            }
        }

        public Task<float[]> GetEmbeddingAsync(string input)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var message = new MessageParameters
                {
                    Model = _options.Model,
                    MaxTokens = 1,
                    Messages = new List<Message>
                    {
                        new Message(RoleType.User, "test")
                    }
                };

                var response = await _client.Messages.GetClaudeMessageAsync(message);
                return response != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
