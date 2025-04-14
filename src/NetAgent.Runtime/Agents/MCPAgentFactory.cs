using NetAgent.Abstractions;
using NetAgent.Abstractions.Models;
using NetAgent.Abstractions.Tools;
using NetAgent.Abstractions.LLM;
using NetAgent.Core.Contexts;
using NetAgent.Core.Memory;
using NetAgent.Core.Planning;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Optimization.Interfaces;
using NetAgent.Runtime.PostProcessing;
using NetAgent.Strategy;
using Microsoft.Extensions.DependencyInjection;
using NetAgent.LLM.Monitoring;

namespace NetAgent.Runtime.Agents
{
    public class MCPAgentFactory : IAgentFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MCPAgentFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IAgent> CreateAgent(AgentOptions options)
        {
            var llm = _serviceProvider.GetRequiredService<IMultiLLMProvider>();
            var tools = _serviceProvider.GetServices<IAgentTool>().ToArray();
            var memory = _serviceProvider.GetRequiredService<IKeyValueMemoryStore>();
            var planner = _serviceProvider.GetRequiredService<IAgentPlanner>();
            var contextSource = _serviceProvider.GetRequiredService<IContextSource>();
            var postProcessor = _serviceProvider.GetRequiredService<IAgentPostProcessor>();
            var strategy = _serviceProvider.GetRequiredService<IAgentStrategy>();
            var evaluator = _serviceProvider.GetRequiredService<IEvaluator>();
            var optimizer = _serviceProvider.GetRequiredService<IOptimizer>();
            var healthCheck = _serviceProvider.GetRequiredService<ILLMHealthCheck>();

            return await Task.FromResult(new MCPAgentBuilder()
                .WithMultiLLM(llm)
                .WithTools(tools)
                .WithKeyValueMemory(memory)
                .WithPlanner(planner)
                .WithContextSource(contextSource)
                .WithPostProcessor(postProcessor)
                .WithStrategy(strategy)
                .WithEvaluator(evaluator)
                .WithOptimizer(optimizer)
                .WithOptions(options)
                .WithHealthCheck(healthCheck)
                .Build());
        }
    }
}