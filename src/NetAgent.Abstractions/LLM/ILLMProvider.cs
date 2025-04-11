using System.Threading.Tasks;

namespace NetAgent.Abstractions.LLM
{
    public interface ILLMProvider
    {
        string Name { get; }
        Task<string> GenerateAsync(string prompt, string goal = "", string context = "");
    }
}