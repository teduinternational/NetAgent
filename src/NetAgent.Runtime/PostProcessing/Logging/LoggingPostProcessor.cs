using NetAgent.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Runtime.PostProcessing.Logging
{
    public class LoggingPostProcessor : IAgentPostProcessor
    {
        public Task PostProcessAsync(AgentResponse result, AgentInputContext context)
        {
            Console.WriteLine("✅ Agent Goal: " + context.Goal);
            Console.WriteLine("📤 Output: " + result.Output);
            return Task.CompletedTask;
        }
    }
}
