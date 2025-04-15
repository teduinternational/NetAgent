using NetAgent.Abstractions.Models;

namespace NetAgent.Core.Memory
{
    public interface ISemanticMemoryStore
    {
        Task SaveAsync(ulong id, string text);
        Task<IReadOnlyList<SemanticSearchResult>> SearchAsync(string query);
    }
}
