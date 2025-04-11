using Microsoft.Extensions.DependencyInjection;
using NetAgent.Strategy.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Strategy.Extensions
{
    public static class StrategyServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultStrategy(this IServiceCollection services)
        {
            return services.AddSingleton<IAgentStrategy, GoalDrivenStrategy>();
        }
    }
}
