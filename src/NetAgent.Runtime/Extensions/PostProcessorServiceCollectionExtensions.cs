using Microsoft.Extensions.DependencyInjection;
using NetAgent.Runtime.Optimization;
using NetAgent.Runtime.PostProcessing.Logging;
using NetAgent.Runtime.PostProcessing;
using NetAgent.Runtime.Options;
using Microsoft.Extensions.Configuration;
using NetAgent.Optimization.Interfaces;
using NetAgent.Optimization.Optimizers;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions;
using NetAgent.Runtime.Agents;
using NetAgent.Evaluation.Evaluators;
using NetAgent.LLM.Providers;
using NetAgent.Evaluation.SelfImproving;

namespace NetAgent.Runtime.Extensions
{
    public static class PostProcessorServiceCollectionExtensions
    {
        public static IServiceCollection AddPostProcessors(this IServiceCollection services, IConfiguration config)
        {
            // Bind option
            var options = new NetAgentPostProcessorOptions();
            config.GetSection("NetAgent:PostProcessors").Bind(options);

            // Conditional registration
            var innerProcessors = new List<IAgentPostProcessor>();

            if (options.EnableLogging)
                services.AddSingleton<IAgentPostProcessor, LoggingPostProcessor>();

            if (options.EnableOptimization)
                services.AddSingleton<IAgentPostProcessor, OptimizationPostProcessor>();

            // Register pipeline after all
            services.AddSingleton<IAgentPostProcessor, AgentPostProcessorPipeline>();

            // Register option itself for reuse
            services.Configure<NetAgentPostProcessorOptions>(config.GetSection("NetAgent:PostProcessors"));
            services.AddSingleton<IEvaluator, LLMEvaluator>();
            services.AddSingleton<IOptimizer, PromptOptimizer>();

            return services;
        }
    }
}
