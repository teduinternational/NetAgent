using NetAgent.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Core.Contexts
{
    public class DefaultContextSource : IContextSource
    {
        public Task<AgentInputContext> GetContextAsync()
        {
            return Task.FromResult(new AgentInputContext
            {
                Context = "No external context available.",
                Metadata = new Dictionary<string, string> { { "Env", "Local" } }
            });
        }
    }
}
