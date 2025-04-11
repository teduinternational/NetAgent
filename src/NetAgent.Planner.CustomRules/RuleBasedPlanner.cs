using NetAgent.Abstractions.Models;
using NetAgent.Core.Planning;
using System.Data;

namespace NetAgent.Planner.CustomRules
{
    public record Rule(string Keyword, string Action);

    public class RuleBasedPlanner : IAgentPlanner
    {
        private readonly List<Rule> _rules;

        public RuleBasedPlanner(IEnumerable<Rule>? rules = null)
        {
            _rules = rules?.ToList() ?? GetDefaultRules();
        }

        public Task<Plan> PlanNextStepAsync(string goal, AgentInputContext context)
        {
            foreach (var rule in _rules)
            {
                if (goal.Contains(rule.Keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(new Plan { Goal = rule.Action });
                }
            }

            return Task.FromResult(new Plan { Goal = "Default rule-based plan: Try to understand the goal step by step." });
        }

        private List<Rule> GetDefaultRules() => new()
        {
            new Rule("weather", "Use WeatherTool to fetch current weather."),
            new Rule("http", "Use HttpGetTool to call external API."),
            new Rule("time", "Use DateTimeTool to get current time."),
        };
    }

}
