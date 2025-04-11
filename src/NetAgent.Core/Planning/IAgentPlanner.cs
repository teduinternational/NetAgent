using NetAgent.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Core.Planning
{
    public interface IAgentPlanner
    {
        Task<string> PlanNextStepAsync(string goal, AgentInputContext context);
    }
}
