using Microsoft.Extensions.DependencyInjection;
using NetAgent.Runtime.Optimization;
using NetAgent.Runtime.PostProcessing.Logging;
using NetAgent.Runtime.PostProcessing;
using NetAgent.Runtime.Options;
using Microsoft.Extensions.Configuration;
using NetAgent.Optimization.Interfaces;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Evaluation.Evaluators;
using NetAgent.Optimization.Optimizers;

namespace NetAgent.Runtime.Extensions
{
    public static class PostProcessorServiceCollectionExtensions
    {
        public static IServiceCollection AddPostProcessors(this IServiceCollection services, IConfiguration config)
        {
            // Bind option
            var options = new NetAgentPostProcessorOptions();
            config.GetSection("NetAgent:PostProcessors").Bind(options);

            // Register individual processors with a different service type to avoid circular dependency
            if (options.EnableLogging)
                services.AddSingleton<LoggingPostProcessor>();

            if (options.EnableOptimization)
                services.AddSingleton<OptimizationPostProcessor>();

            // Register the pipeline as IAgentPostProcessor, injecting the concrete types
            services.AddSingleton<IAgentPostProcessor>(sp =>
            {
                var processors = new List<IAgentPostProcessor>();
                
                if (options.EnableLogging)
                    processors.Add(sp.GetRequiredService<LoggingPostProcessor>());
                    
                if (options.EnableOptimization)
                    processors.Add(sp.GetRequiredService<OptimizationPostProcessor>());

                return new AgentPostProcessorPipeline(processors);
            });

            // Register option itself for reuse
            services.Configure<NetAgentPostProcessorOptions>(config.GetSection("NetAgent:PostProcessors"));
            services.AddSingleton<IEvaluator, DefaultEvaluator>();
            services.AddSingleton<IOptimizer, DefaultOptimizer>();

            return services;
        }
    }
}
