using System.Collections.Generic;
using System.Threading.Tasks;
using NetAgent.Abstractions.Models;

namespace NetAgent.Abstractions.LLM
{
    public interface IStreamingLLMProvider : ILLMProvider
    {
        /// <summary>
        /// Generates a streaming response from the LLM
        /// </summary>
        /// <param name="prompt">The prompt to send to the LLM</param>
        /// <returns>An async enumerable of response chunks</returns>
        IAsyncEnumerable<LLMResponseChunk> GenerateStreamAsync(Prompt prompt);

        /// <summary>
        /// Checks if the provider supports streaming for the given model
        /// </summary>
        bool SupportsStreaming { get; }
    }

    public class LLMResponseChunk
    {
        /// <summary>
        /// The content of this chunk
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Whether this is the final chunk
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Any additional metadata for this chunk
        /// </summary>
        public IDictionary<string, object> Metadata { get; set; }
    }
}