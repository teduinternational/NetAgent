using System.Threading.Tasks;

namespace NetAgent.Abstractions.LLM
{
    public interface ILLMProvider
    {
        string Name { get; }

        /// <summary>
        /// Sinh một kết quả đầu ra duy nhất từ prompt.
        /// </summary>
        Task<string> GenerateAsync(string prompt);
    }
}