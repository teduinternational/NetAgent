using NetAgent.Core.Memory;
using NetAgent.Memory.SemanticQdrant.Models;
using NetAgent.Abstractions.Models;
using Google.Protobuf.Collections;
using System.Net;
using System.Net.Http.Json;
using Qdrant.Client.Grpc;

namespace NetAgent.Memory.SemanticQdrant
{
    public class QdrantSemanticMemory : ISemanticMemoryStore
    {
        private readonly HttpClient HttpClient;
        private readonly QdrantOptions _options;
        private readonly IEmbeddingProvider _embeddingProvider;

        public QdrantSemanticMemory(QdrantOptions options, IEmbeddingProvider embeddingProvider)
        {
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
            _options = options;
            _embeddingProvider = embeddingProvider;
        }

        public async Task SaveAsync(ulong id, string text)
        {
            var embedding = await _embeddingProvider.GetEmbeddingAsync(text);

            var upsertPayload = new
            {
                points = new[]
                {
                    new
                    {
                        id = id,
                        vector = embedding
                    }
                }
            };

            var upsertResponse = await HttpClient.PutAsJsonAsync(
                $"{_options.Endpoint}/collections/{_options.CollectionName}/points",
                upsertPayload
            );

            if (!upsertResponse.IsSuccessStatusCode)
            {
                throw new Exception("Failed to upsert vector");
            }
        }

        public async Task<IReadOnlyList<SemanticSearchResult>> SearchAsync(string query)
        {
            var embedding = await _embeddingProvider.GetEmbeddingAsync(query);

            var searchPayload = new
            {
                query = embedding,
                with_payload = true
            };

            var searchResponse = await HttpClient.PostAsJsonAsync(
                $"{_options.Endpoint}/collections/{_options.CollectionName}/points/query",
                searchPayload
            );

            if (!searchResponse.IsSuccessStatusCode)
            {
                throw new Exception("Failed to search vectors");
            }

            var searchResult = await searchResponse.Content.ReadFromJsonAsync<List<ScoredPoint>>();

            return searchResult
                .Select(FromScoredPoint)
                .ToList();
        }

        public static SemanticSearchResult FromScoredPoint(ScoredPoint point)
        {
            return new SemanticSearchResult
            {
                Id = point.Id.ToString(),
                Score = point.Score,
                Payload = ConvertPayload(point.Payload)
            };
        }


        public static Dictionary<string, object> ConvertPayload(MapField<string, Value> payload)
        {
            var result = new Dictionary<string, object>();

            foreach (var kv in payload)
            {
                result[kv.Key] = ConvertValue(kv.Value);
            }

            return result;
        }

        private static object ConvertValue(Value value)
        {
            switch (value.KindCase)
            {
                case Value.KindOneofCase.StringValue:
                    return value.StringValue;
                case Value.KindOneofCase.DoubleValue:
                    return value.DoubleValue;
                case Value.KindOneofCase.IntegerValue:
                    return value.IntegerValue;
                case Value.KindOneofCase.BoolValue:
                    return value.BoolValue;
                case Value.KindOneofCase.ListValue:
                    return value.ListValue.Values.Select(ConvertValue).ToList();
                case Value.KindOneofCase.StructValue:
                    return value.StructValue.Fields.ToDictionary(f => f.Key, f => ConvertValue(f.Value));
                case Value.KindOneofCase.NullValue:
                    return null;
                default:
                    return value.ToString(); // fallback
            }
        }
    }
}
