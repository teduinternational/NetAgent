using NetAgent.LLM.Interfaces;
using System.Text;
using System.Text.Json;

namespace NetAgent.LLM.Ollama
{
    public class OllamaProvider : ILLMProvider
    {
        private readonly OllamaOptions _options;
        private readonly HttpClient _httpClient;
        public string Name { get { return "Ollama"; } }

        public OllamaProvider(OllamaOptions options, HttpClient? httpClient = null)
        {
            _options = options;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var requestBody = new
            {
                model = _options.Model,
                prompt = prompt,
                stream = false
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.Host}/api/generate");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
        }
    }
}
