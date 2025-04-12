using Microsoft.Extensions.Logging;
using NetAgent.Abstractions.Models;

namespace NetAgent.Abstractions.LLM
{
    public interface IMultiLLMProvider : ILLMProvider
    {
        /// <summary>
        /// Sinh đầu ra từ tất cả các LLM đang được cấu hình.
        /// </summary>
        Task<LLMResponse[]> GenerateFromAllAsync(Prompt prompt);

        /// <summary>
        /// Sinh kết quả tốt nhất từ các đầu ra theo voting/evaluation.
        /// </summary>
        Task<LLMResponse> GenerateBestAsync(Prompt prompt);

        /// <summary>
        /// Lấy danh sách các provider.
        /// </summary>
        IEnumerable<ILLMProvider> GetProviders();

        /// <summary>
        /// Lấy scorer để đánh giá kết quả.
        /// </summary>
        IResponseScorer GetScorer();

        /// <summary>
        /// Lấy logger để ghi log.
        /// </summary>
        ILogger<IMultiLLMProvider> GetLogger();
    }
}