namespace NetAgent.Core.Memory
{
    public interface ISemanticMemoryStore
    {
        Task SaveAsync(string text, string id = null);
        Task<IReadOnlyList<(string text, float score)>> SearchAsync(string query, int topK = 3, float minScore = 0.75f);
    }
}
