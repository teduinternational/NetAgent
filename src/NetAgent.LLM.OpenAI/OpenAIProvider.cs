using NetAgent.Abstractions.LLM;
using OpenAI_API;
using System;
using System.Threading.Tasks;

namespace NetAgent.LLM.OpenAI
{
    public class OpenAIProvider : ILLMProvider
    {
        private readonly OpenAIAPI _api;
        private readonly OpenAIOptions _options;

        public OpenAIProvider(OpenAIOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _api = new OpenAIAPI(new APIAuthentication(_options.ApiKey));
        }

        public string Name => "OpenAI";
        public async Task<string> GenerateAsync(string prompt, string goal = "", string context = "")
        {
            try
            {
                var chat = _api.Chat.CreateConversation();
                chat.Model = _options.Model ?? "gpt-3.5-turbo";
                chat.RequestParameters.Temperature = _options.Temperature ?? 0.7;
                chat.RequestParameters.MaxTokens = _options.MaxTokens ?? 2000;

                chat.AppendUserInput(prompt);

                string response = await chat.GetResponseFromChatbotAsync();
                return response;
            }
            catch (Exception ex)
            {
                throw new LLMException($"OpenAI API error: {ex.Message}", ex);
            }
        }
    }
}
