using NetAgent.Core.Memory;
using StackExchange.Redis;

namespace NetAgent.Memory.Redis
{
    public class RedisMemoryStore : IKeyValueMemoryStore
    {
        private readonly IDatabase _db;
        private const string KeyPrefix = "netagent:memory:";

        public RedisMemoryStore(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SaveAsync(string goal, string response)
        {
            await _db.StringSetAsync(KeyPrefix + goal, response);
        }

        public async Task<string?> RetrieveAsync(string key)
        {
            var value = await _db.StringGetAsync(KeyPrefix + key);
            return value.HasValue ? value.ToString() : null;
        }
    }
}
