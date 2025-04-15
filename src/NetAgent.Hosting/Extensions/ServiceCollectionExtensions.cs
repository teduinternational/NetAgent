using Microsoft.Extensions.Configuration;
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
using NetAgent.Planner.Default;
using NetAgent.Memory.InMemory;
using NetAgent.Runtime.PostProcessing;
using NetAgent.Strategy;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Optimization.Interfaces;
using NetAgent.LLM.Extensions;
using NetAgent.Runtime.Extensions;
using NetAgent.Strategy.Extensions;
using NetAgent.Tools.Standard.TavilySearch;
using NetAgent.LLM.OpenAI.Extensions;
using NetAgent.LLM.Claude.Extensions;
using NetAgent.LLM.DeepSeek.Extensions;
using NetAgent.LLM.Gemini.Extensions;
using NetAgent.LLM.Monitoring;
using NetAgent.Memory.SemanticQdrant;
using NetAgent.Memory.SemanticQdrant.Extensions;

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

        public static IServiceCollection AddNetAgent(
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

            services.AddLLMProviders(configuration);
            services.AddMemoryProviders(configuration);
            services.AddPlanners(configuration);
            services.AddContextSource(configuration);
            services.AddMultiLLMProviders();
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

            services.AddAgentTools(configuration);

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
            services.AddTransient(sp =>
            {
                try
                {
                    var multiLLM = sp.GetRequiredService<IMultiLLMProvider>();
                    var tools = sp.GetServices<IAgentTool>().ToArray();
                    var memory = sp.GetRequiredService<IKeyValueMemoryStore>();
                    var planner = sp.GetRequiredService<IAgentPlanner>();
                    var context = sp.GetRequiredService<IContextSource>();
                    var postProcessor = sp.GetRequiredService<IAgentPostProcessor>();
                    var strategy = sp.GetRequiredService<IAgentStrategy>();
                    var evaluator = sp.GetRequiredService<IEvaluator>();
                    var optimizer = sp.GetRequiredService<IOptimizer>();
                    var healthCheck = sp.GetRequiredService<ILLMHealthCheck>();
                    if (!tools.Any())
                    {
                        throw new ConfigurationException("No agent tools were registered");
                    }

                    return new MCPAgentBuilder()
                        .WithMultiLLM(multiLLM)
                        .WithTools(tools)
                        .WithKeyValueMemory(memory)
                        .WithPlanner(planner)
                        .WithContextSource(context)
                        .WithPostProcessor(postProcessor)
                        .WithStrategy(strategy)
                        .WithEvaluator(evaluator)
                        .WithOptimizer(optimizer)
                        .WithHealthCheck(healthCheck)
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

        public static IServiceCollection AddLLMProviders(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var options = configuration.GetSection(CONFIG_LLM_KEY).Get<LLMOptions>() ?? new();
            var providers = options.Providers ?? new[] { options.Provider?.ToLowerInvariant() };

            foreach (var provider in providers.Where(p => !string.IsNullOrEmpty(p)))
            {
                switch (provider.ToLowerInvariant())
                {
                    case "openai":
                        if (options.OpenAI is not null)
                            services.AddOpenAIProvider(options.OpenAI);
                        break;

                    case "claude":
                        if (options.Claude is not null)
                            services.AddClaudeAIProvider(options.Claude);
                        break;

                    case "deepseek":
                        if (options.DeepSeek is not null)
                            services.AddDeepSeekProvider(options.DeepSeek);
                        break;

                    case "gemini":
                        if (options.Gemini is not null)
                            services.AddGeminiProvider(options.Gemini);
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported LLM Provider: {provider}");
                }
            }

            return services;
        }

        public static IServiceCollection AddPlanners(
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

        public static IServiceCollection AddMemoryProviders(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var type = configuration[CONFIG_MEMORY_KEY]?.ToLowerInvariant() ?? "inmemory";
            return type switch
            {
                "inmemory" => services.AddSingleton<IKeyValueMemoryStore, InMemoryMemoryStore>(),
                "qdrant" => services.AddQdrantSemanticMemory(configuration),
                _ => throw new InvalidOperationException($"Unknown memory store type: {type}"),
            };
        }

        public static IServiceCollection AddContextSource(
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

        public static IServiceCollection AddAgentTools(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var toolNames = configuration.GetSection(CONFIG_TOOLS_KEY).Get<string[]>() ?? Array.Empty<string>();

            foreach (var name in toolNames)
            {
                switch (name.ToLowerInvariant())
                {
                    case "tavilysearch":
                        services.Configure<TavilySearchOptions>(configuration.GetSection("NetAgent:Tools:TavilySearch"));
                        services.AddSingleton<IAgentTool>(sp =>
                        {
                            var tavilyOptions = sp.GetRequiredService<IOptions<TavilySearchOptions>>().Value;
                            return new TavilySearchTool(tavilyOptions);
                        });
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown tool: {name}");
                }
            }

            return services;
        }
    }
}
