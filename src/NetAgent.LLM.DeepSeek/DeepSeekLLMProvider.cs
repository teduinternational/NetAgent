using System.Net.Http.Json;
using System.Text.Json;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.DeepSeek
{
    public class DeepSeekLLMProvider : ILLMProvider
    {
        private readonly DeepSeekOptions _options;
        private readonly HttpClient _httpClient;
        private const string API_URL = "https://api.deepseek.com/v1/chat/completions";

        public DeepSeekLLMProvider(DeepSeekOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        }

        public string Name => "DeepSeek";

        public async Task<string> GenerateAsync(string prompt, string goal = "", string context = "")
        {
            var messages = new List<object>();
            
            if (!string.IsNullOrEmpty(context))
            {
                messages.Add(new { role = "system", content = context });
            }
            
            if (!string.IsNullOrEmpty(goal))
            {
                messages.Add(new { role = "system", content = goal });
            }

            messages.Add(new { role = "user", content = prompt });

            var requestBody = new
            {
                model = _options.Model,
                messages,
                temperature = _options.Temperature,
                max_tokens = _options.MaxTokens
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(API_URL, requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                var generatedText = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                return generatedText ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate response from DeepSeek API: {ex.Message}", ex);
            }
        }
    }
}
