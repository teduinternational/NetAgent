﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetAgent.Runtime.Agents;
using NetAgent.Hosting.Options;
using NetAgent.Abstractions;
using NetAgent.Abstractions.Tools;
using NetAgent.Abstractions.LLM;
using NetAgent.Core.Memory;
using NetAgent.Core.Planning;
using NetAgent.Core.Contexts;
using NetAgent.Core.Exceptions;
using NetAgent.Tools.Standard;
using NetAgent.Planner.Default;
using NetAgent.Memory.InMemory;
using NetAgent.Planner.Extensions;
using NetAgent.Runtime.PostProcessing;
using NetAgent.Strategy;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Optimization.Interfaces;
using NetAgent.LLM.Extensions;
using NetAgent.Evaluation.Evaluators;
using NetAgent.LLM.Scoring;
using NetAgent.Runtime.Extensions;
using NetAgent.Strategy.Extensions;

namespace NetAgent.Hosting.Extensions
{
    /// <summary>
    /// Extension methods for configuring NetAgent services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private const string CONFIG_ROOT_KEY = "NetAgent";
        private const string CONFIG_LLM_KEY = "NetAgent:LLM";
        private const string CONFIG_TOOLS_KEY = "NetAgent:Tools:Types";
        private const string CONFIG_PLANNER_KEY = "NetAgent:Planner:Type";
        private const string CONFIG_MEMORY_KEY = "NetAgent:Memory:Type";
        private const string CONFIG_CONTEXT_KEY = "NetAgent:Context:Type";

        public static IServiceCollection AddNetAgentFromConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var logger = services.BuildServiceProvider().GetService<ILogger<NetAgentOptions>>();
            
            try
            {
                logger?.LogInformation("Configuring NetAgent services...");

                var options = configuration.GetSection(CONFIG_ROOT_KEY).Get<NetAgentOptions>();
                if (options == null)
                {
                    throw new ConfigurationException($"Missing {CONFIG_ROOT_KEY} configuration section");
                }

                services.Configure<NetAgentOptions>(configuration.GetSection(CONFIG_ROOT_KEY));
                services.AddSingleton<IValidateOptions<NetAgentOptions>, NetAgentOptionsValidator>();

                RegisterProviders(services, configuration, logger);
                RegisterTools(services, configuration, logger);
                RegisterAgent(services, logger);

                logger?.LogInformation("NetAgent services configured successfully");
                
                return services;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to configure NetAgent services");
                throw new ConfigurationException("Failed to configure NetAgent services", ex);
            }
        }

        private static void RegisterProviders(
            IServiceCollection services,
            IConfiguration configuration,
            ILogger? logger)
        {
            logger?.LogInformation("Registering providers...");

            services.AddLLMProvidersFromConfig(configuration);
            services.AddMemoryProviderFromConfig(configuration);
            services.AddPlannerFromConfig(configuration);
            services.AddContextSourceFromConfig(configuration);
            services.AddMultiLLMProvider();
            services.AddPostProcessors(configuration);
            services.AddDefaultStrategy();
            logger?.LogInformation("Providers registered successfully");
        }

        private static void RegisterTools(
            IServiceCollection services, 
            IConfiguration configuration,
            ILogger? logger)
        {
            logger?.LogInformation("Registering agent tools...");

            services.AddAgentToolsFromConfig(configuration);

            logger?.LogInformation("Agent tools registered successfully");
        }

        private static void RegisterAgent(
            IServiceCollection services,
            ILogger? logger)
        {
            logger?.LogInformation("Registering agent and factory...");

            // Register IAgentFactory
            services.AddSingleton<IAgentFactory, MCPAgentFactory>();

            // Register IAgent as transient for multiple instances
            services.AddTransient<IAgent>(sp =>
            {
                try
                {
                    var llm = sp.GetRequiredService<IMultiLLMProvider>();
                    var tools = sp.GetServices<IAgentTool>().ToArray();
                    var memory = sp.GetRequiredService<IMemoryStore>();
                    var planner = sp.GetRequiredService<IAgentPlanner>();
                    var context = sp.GetRequiredService<IContextSource>();
                    var postProcessor = sp.GetRequiredService<IAgentPostProcessor>();
                    var strategy = sp.GetRequiredService<IAgentStrategy>();
                    var evaluator = sp.GetRequiredService<IEvaluator>();
                    var optimizer = sp.GetRequiredService<IOptimizer>();

                    if (!tools.Any())
                    {
                        throw new ConfigurationException("No agent tools were registered");
                    }

                    return new MCPAgentBuilder()
                        .WithMultiLLM(llm)
                        .WithTools(tools)
                        .WithMemory(memory)
                        .WithPlanner(planner)
                        .WithContextSource(context)
                        .WithPostProcessor(postProcessor)
                        .WithStrategy(strategy)
                        .WithEvaluator(evaluator)
                        .WithOptimizer(optimizer)
                        .Build();
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to create agent instance");
                    throw;
                }
            });

            logger?.LogInformation("Agent and factory registered successfully");
        }

        public static IServiceCollection AddLLMProvidersFromConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var options = configuration.GetSection(CONFIG_LLM_KEY).Get<LLMOptions>() ?? new();

            switch (options.Provider?.ToLowerInvariant())
            {
                case "openai":
                    if (options.OpenAI is not null)
                        services.AddOpenAIProvider(options.OpenAI);
                    break;

                case "azureopenai":
                    if (options.AzureOpenAI is not null)
                        services.AddAzureOpenAIProvider(options.AzureOpenAI);
                    break;

                case "ollama":
                    if (options.Ollama is not null)
                        services.AddOllamaProvider(options.Ollama);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported LLM Provider: {options.Provider}");
            }

            return services;
        }

        public static IServiceCollection AddPlannerFromConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var type = configuration[CONFIG_PLANNER_KEY]?.ToLowerInvariant() ?? "default";
            return type switch
            {
                "default" => services.AddSingleton<IAgentPlanner, DefaultPlanner>(),
                _ => throw new InvalidOperationException($"Unknown planner type: {type}")
            };
        }

        public static IServiceCollection AddMemoryProviderFromConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var type = configuration[CONFIG_MEMORY_KEY]?.ToLowerInvariant() ?? "inmemory";
            return type switch
            {
                "inmemory" => services.AddSingleton<IMemoryStore, InMemoryMemoryStore>(),
                _ => throw new InvalidOperationException($"Unknown memory store type: {type}")
            };
        }

        public static IServiceCollection AddContextSourceFromConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var type = configuration[CONFIG_CONTEXT_KEY]?.ToLowerInvariant() ?? "default";
            return type switch
            {
                "default" => services.AddSingleton<IContextSource, DefaultContextSource>(),
                _ => throw new InvalidOperationException($"Unknown context source type: {type}")
            };
        }

        public static IServiceCollection AddAgentToolsFromConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var toolNames = configuration.GetSection(CONFIG_TOOLS_KEY).Get<string[]>() ?? Array.Empty<string>();

            foreach (var name in toolNames)
            {
                switch (name.ToLowerInvariant())
                {
                    case "websearch":
                        services.AddSingleton<IAgentTool, WebSearchTool>();
                        break;
                    case "calculator":
                        services.AddSingleton<IAgentTool, CalculatorTool>();
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown tool: {name}");
                }
            }

            return services;
        }
    }
}
