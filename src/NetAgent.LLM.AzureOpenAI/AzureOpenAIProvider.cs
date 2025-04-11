using NetAgent.LLM.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NetAgent.LLM.AzureOpenAI
{
    public class AzureOpenAIProvider : ILLMProvider
    {
        private readonly AzureOpenAIOptions _options;
        private readonly HttpClient _httpClient;

        public AzureOpenAIProvider(AzureOpenAIOptions options, HttpClient? httpClient = null)
        {
            _options = options;
            _httpClient = httpClient ?? new HttpClient();
        }

        public string Name { get { return "AzureOpenAI"; } }

        public async Task<string> GenerateAsync(string prompt)
        {
            var endpoint = $"https://{_options.ResourceName}.openai.azure.com/openai/deployments/{_options.DeploymentName}/chat/completions?api-version={_options.ApiVersion}";

            var body = new
            {
                messages = new[] {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            };

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Headers.Add("api-key", _options.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        }
    }
}
