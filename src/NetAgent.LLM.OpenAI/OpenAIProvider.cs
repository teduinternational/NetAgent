using NetAgent.LLM.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NetAgent.LLM.OpenAI
{
    public class OpenAIProvider : ILLMProvider
    {
        private readonly OpenAIOptions _options;
        private readonly HttpClient _httpClient;
        public string Name { get { return "OpenAI"; } }

        public OpenAIProvider(OpenAIOptions options, HttpClient? httpClient = null)
        {
            _options = options;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var requestBody = new
            {
                model = _options.Model,
                messages = new[] {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        }
    }
}
