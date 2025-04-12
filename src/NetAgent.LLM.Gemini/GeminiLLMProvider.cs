using System.Net.Http.Headers;
using System.Text;
using NetAgent.Abstractions.LLM;
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

        public async Task<string> GenerateAsync(string prompt, string goal = "", string context = "")
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
            return responseString;
        }
    }
}
