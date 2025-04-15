using NetAgent.Abstractions.LLM;

namespace NetAgent.Memory.SemanticQdrant
{
    public interface IEmbeddingProvider
    {
        Task<float[]> GetEmbeddingAsync(string input);
    }

    public class MultiLLMEmbeddingProvider : IEmbeddingProvider
    {
        private readonly IEnumerable<ILLMProvider> _llmProviders;

        public MultiLLMEmbeddingProvider(IEnumerable<ILLMProvider> llmProviders)
        {
            _llmProviders = llmProviders ?? throw new ArgumentNullException(nameof(llmProviders));
        }

        public async Task<float[]> GetEmbeddingAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));
            }

            var embeddingTasks = _llmProviders.Select(provider => provider.GetEmbeddingAsync(input));
            var embeddings = await Task.WhenAll(embeddingTasks);

            // Combine embeddings (e.g., averaging them)
            var combinedEmbedding = new float[embeddings[0].Length];
            foreach (var embedding in embeddings)
            {
                for (int i = 0; i < embedding.Length; i++)
                {
                    combinedEmbedding[i] += embedding[i];
                }
            }

            for (int i = 0; i < combinedEmbedding.Length; i++)
            {
                combinedEmbedding[i] /= embeddings.Length;
            }

            return combinedEmbedding;
        }
    }
}
