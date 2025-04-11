using NetAgent.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Runtime.PostProcessing
{
    public interface IAgentPostProcessor
    {
        Task PostProcessAsync(AgentResult result, AgentInputContext context);
    }
}
