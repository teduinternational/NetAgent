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
            _client = new OpenAIAPI(_options.ApiKey);
        }

        public string Name => "openai";

        public bool SupportsStreaming => true;

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            var completionRequest = new OpenAI_API.Completions.CompletionRequest {
                Prompt = prompt.Content,
                Model = _options.Model,
                MaxTokens = _options.MaxTokens,
                Temperature = _options.Temperature
            };

            var completionResponse = await _client.Completions.CreateCompletionAsync(completionRequest);
            var result = completionResponse.Completions.FirstOrDefault()?.Text;

            if (result == null)
            {
                throw new InvalidOperationException("Failed to generate completion.");
            }

            return new LLMResponse()
            {
                Content = result
            };
        }

        public async IAsyncEnumerable<LLMResponseChunk> GenerateStreamAsync(Prompt prompt)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            var completionRequest = new OpenAI_API.Completions.CompletionRequest
            {
                Prompt = prompt.Content,
                Model = _options.Model,
                MaxTokens = _options.MaxTokens,
                Temperature = _options.Temperature,
            };

            await foreach (var chunk in _client.Completions.StreamCompletionEnumerableAsync(completionRequest))
            {
                yield return new LLMResponseChunk { Content = chunk.ToString() };
            }
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
