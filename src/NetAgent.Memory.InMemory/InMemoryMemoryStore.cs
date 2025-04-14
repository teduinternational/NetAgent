using NetAgent.Core.Memory;

namespace NetAgent.Memory.InMemory
{
    public class InMemoryMemoryStore : IKeyValueMemoryStore
    {
        private readonly Dictionary<string, string> _memory = new();
        private readonly object _lock = new();

        public Task SaveAsync(string goal, string response)
        {
            ArgumentNullException.ThrowIfNull(goal);
            ArgumentNullException.ThrowIfNull(response);

            lock (_lock)
            {
                _memory[goal] = response;
            }
            return Task.CompletedTask;
        }

        public Task<string?> RetrieveAsync(string key)
        {
            ArgumentNullException.ThrowIfNull(key);
            
            lock (_lock)
            {
                return Task.FromResult(_memory.TryGetValue(key, out var value) ? value : null);
            }
        }
    }
}
