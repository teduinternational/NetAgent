using System.Net.Http.Json;
using System.Text.Json;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;

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

        public string Name => "deepseek";

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            var messages = new List<object>()
            {
                new { role = "user", content = prompt }
            };

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

                return new LLMResponse()
                {
                    Content = generatedText ?? string.Empty,
                    TokensUsed = result.GetProperty("usage").GetProperty("total_tokens").GetInt32(),
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate response from DeepSeek API: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://api.deepseek.com/v1/models");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<float[]> GetEmbeddingAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));
            }

            var requestBody = new
            {
                model = _options.EmbeddingModel,
                input
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.deepseek.com/v1/embeddings", requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var embedding = result.GetProperty("data")[0].GetProperty("embedding").EnumerateArray()
                .Select(x => x.GetSingle()).ToArray();

            return embedding;
        }
    }
}
