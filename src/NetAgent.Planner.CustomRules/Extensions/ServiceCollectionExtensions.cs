using Microsoft.Extensions.DependencyInjection;
using NetAgent.Core.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Planner.CustomRules.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRuleBasedPlanner(this IServiceCollection services, IEnumerable<Rule>? rules = null)
        {
            services.AddSingleton<IAgentPlanner>(new RuleBasedPlanner(rules));
            return services;
        }
    }
}
