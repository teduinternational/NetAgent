using Qdrant.Client;
using NetAgent.Core.Memory;
using Qdrant.Client.Grpc;
using System.Net.Http.Json;
using System.Text.Json;
using NetAgent.Memory.SemanticQdrant.Models;

namespace NetAgent.Memory.SemanticQdrant
{
    public class QdrantSemanticMemory : ISemanticMemoryStore
    {
        private readonly HttpClient _http;
        private readonly IEmbeddingProvider _embedding;
        private readonly QdrantOptions _options;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public QdrantSemanticMemory(HttpClient http, IEmbeddingProvider embedding, QdrantOptions options)
        {
            _http = http;
            _embedding = embedding;
            _options = options;
        }

        public async Task SaveAsync(string text, string? id = null)
        {
            var embedding = await _embedding.GetEmbeddingAsync(text);
            var point = new
            {
                id = id ?? Guid.NewGuid().ToString(),
                vector = embedding,
                payload = new { text }
            };

            var body = new { points = new[] { point } };
            var url = $"{_options.Endpoint}/collections/{_options.CollectionName}/points?wait=true";
            await _http.PutAsJsonAsync(url, body, _jsonOptions);
        }

        public async Task<IReadOnlyList<(string text, float score)>> SearchAsync(string query, int topK = 3, float minScore = 0.75f)
        {
            var embedding = await _embedding.GetEmbeddingAsync(query);
            var body = new
            {
                vector = embedding,
                limit = topK,
                score_threshold = minScore,
                with_payload = true
            };

            var url = $"{_options.Endpoint}/collections/{_options.CollectionName}/points/search";
            var response = await _http.PostAsJsonAsync(url, body, _jsonOptions);
            var result = await response.Content.ReadFromJsonAsync<QdrantSearchResult>(_jsonOptions);

            return result?.result
                ?.Select(r => ((string)r.payload["text"], r.score))
                .ToList() ?? new List<(string, float)>();
        }
    }
}
