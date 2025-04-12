using NetAgent.Abstractions.Models;

namespace NetAgent.Abstractions.LLM
{
    public interface ILLMProvider
    {
        string Name { get; }
        Task<LLMResponse> GenerateAsync(Prompt prompt);
    }
}