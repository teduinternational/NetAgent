using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.Grok
{
    public class GrokLLMProvider : ILLMProvider
    {
        private readonly GrokOptions _options;
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "https://api.grok.x.ai/v1";

        public GrokLLMProvider(GrokOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        }

        public string Name => "Grok";

        public async Task<string> GenerateAsync(string prompt, string goal = "", string context = "")
        {
            try
            {
                var requestBody = new
                {
                    model = _options.Model,
                    messages = new[]
                    {
                        new { role = "system", content = !string.IsNullOrEmpty(goal) ? goal : "You are a helpful AI assistant." },
                        new { role = "user", content = $"{context}\n{prompt}".Trim() }
                    },
                    temperature = _options.Temperature,
                    max_tokens = _options.MaxTokens
                };

                var response = await _httpClient.PostAsJsonAsync($"{API_BASE_URL}/chat/completions", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() 
                    ?? throw new Exception("No content in response");
            }
            catch (Exception ex)
            {
                throw new LLMException($"Grok API call failed: {ex.Message}", ex);
            }
        }
    }
}
