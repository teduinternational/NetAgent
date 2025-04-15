using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using OpenAI_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetAgent.LLM.OpenAI
{
    public class OpenAIProvider : IStreamingLLMProvider
    {
        private readonly IOpenAIAPI _client;
        private readonly OpenAIOptions _options;

        public OpenAIProvider(OpenAIOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _client = new OpenAI_API.OpenAIAPI(_options.ApiKey);
        }

        public string Name => "openai";

        public bool SupportsStreaming => true;

        public Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<LLMResponseChunk> GenerateStreamAsync(Prompt prompt)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var models = await _client.Models.GetModelsAsync();
                return models != null && models.Any();
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

            var embeddingRequest = new OpenAI_API.Embedding.EmbeddingRequest
            {
                Input = input,
                Model = _options.EmbeddingModel
            };

            var embeddingResponse = await _client.Embeddings.CreateEmbeddingAsync(embeddingRequest);
            return embeddingResponse.Data.FirstOrDefault()?.Embedding ?? throw new InvalidOperationException("Failed to generate embedding.");
        }
    }
}
