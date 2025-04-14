using System.Net.Http.Headers;
using System.Text;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using Newtonsoft.Json;

namespace NetAgent.LLM.Gemini
{
    public class GeminiLLMProvider : ILLMProvider
    {
        private readonly GeminiOptions _options;
        private readonly HttpClient _httpClient;
        public GeminiLLMProvider(GeminiOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        }

        public string Name => "Gemini";

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent",
                content
            );

            var responseString = await response.Content.ReadAsStringAsync();
            return new LLMResponse()
            {
                Content = responseString,
                ModelName = _options.Model,
                TokensUsed = 0, // Gemini API does not provide token usage in the response
            };
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}"
                );
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
