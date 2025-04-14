using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Core.Memory
{
    public interface IKeyValueMemoryStore
    {
        Task SaveAsync(string key, string value);
        Task<string?> RetrieveAsync(string key);
    }
}
