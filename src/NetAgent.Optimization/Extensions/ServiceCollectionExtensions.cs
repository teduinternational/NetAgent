using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetAgent.Optimization.Interfaces;
using NetAgent.Optimization.Optimizers;

namespace NetAgent.Optimization.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOptimizers(this IServiceCollection services, 
            IConfiguration configuration)
        {
            var type = configuration["NetAgent:Optimizer:Type"]?.ToLowerInvariant() ?? "prompt";

            return type switch
            {
                "prompt" => services.AddSingleton<IOptimizer, DefaultOptimizer>(),
                "dummy" => services.AddSingleton<IOptimizer, DummyOptimizer>(),
                _ => throw new InvalidOperationException($"Unknown optimizer type: {type}")
            };
        }
    }
}
