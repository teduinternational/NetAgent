using System.Text.Json;
using System.Text.Json.Serialization;
using NetAgent.Abstractions.Tools;

namespace NetAgent.Tools.Standard.TavilySearch
{
    public class TavilySearchTool : IAgentTool
    {
        private readonly HttpClient _httpClient;
        private readonly TavilySearchOptions _options;
        public TavilySearchTool(TavilySearchOptions options)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.tavily.com/")
            };
            _options = options;
        }

        public string Name => "tavilysearch";

        public async Task<string> ExecuteAsync(string input)
        {
            var request = new
            {
                api_key = _options.ApiKey,
                query = input,
                search_depth = "basic",
                include_images = false,
                include_answer = true
            };

            var response = await _httpClient.PostAsync("search", 
                new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json"));
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var searchResult = JsonSerializer.Deserialize<TavilySearchResponse>(content);
            
            return searchResult?.Answer ?? $"No results found for: {input}";
        }
    }

    internal class TavilySearchResponse
    {
        [JsonPropertyName("answer")]
        public string Answer { get; set; }
    }
}
