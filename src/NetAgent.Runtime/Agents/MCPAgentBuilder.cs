using NetAgent.Abstractions;
using NetAgent.Abstractions.Tools;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.Core.Contexts;
using NetAgent.Core.Memory;
using NetAgent.Core.Planning;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Optimization.Interfaces;
using NetAgent.Runtime.PostProcessing;
using NetAgent.Strategy;
using NetAgent.Memory.InMemory;
using NetAgent.Runtime.Optimization;
using NetAgent.Optimization.Optimizers;
using NetAgent.Evaluation.Evaluators;
using NetAgent.Strategy.Strategies;
using NetAgent.Planner.Default;
using NetAgent.LLM.Providers;
using NetAgent.LLM.Monitoring;
using NetAgent.Memory.SemanticQdrant;
using NetAgent.Memory.SemanticQdrant.Models;

namespace NetAgent.Runtime.Agents
{
    public class MCPAgentBuilder
    {
        private QdrantOptions _qdrantOptions;
        private ILLMProvider? _llm;
        private IEnumerable<IAgentTool>? _tools;
        private IAgentPlanner? _planner;
        private IContextSource? _contextSource;
        private IKeyValueMemoryStore? _keyValueMemory;
        private ISemanticMemoryStore? _semanticMemory;
        private IAgentPostProcessor? _postProcessor;
        private IAgentStrategy? _strategy;
        private IMultiLLMProvider? _multiLLMProvider;
        private IEvaluator? _evaluator;
        private IOptimizer? _optimizer;
        private ILLMHealthCheck? _healthCheck;
        private AgentOptions? _options;

        public MCPAgentBuilder WithLLM(ILLMProvider llm)
        {
            _llm = llm;
            return this;
        }

        public MCPAgentBuilder WithTools(IEnumerable<IAgentTool> tools)
        {
            _tools = tools;
            return this;
        }

        public MCPAgentBuilder WithPlanner(IAgentPlanner planner)
        {
            _planner = planner;
            return this;
        }

        public MCPAgentBuilder WithContextSource(IContextSource contextSource)
        {
            _contextSource = contextSource;
            return this;
        }

        public MCPAgentBuilder WithKeyValueMemory(IKeyValueMemoryStore memory)
        {
            _keyValueMemory = memory;
            return this;
        }
        public MCPAgentBuilder WithSemanticMemory(ISemanticMemoryStore memory)
        {
            _semanticMemory = memory;
            return this;
        }

        public MCPAgentBuilder WithPostProcessor(IAgentPostProcessor postProcessor)
        {
            _postProcessor = postProcessor;
            return this;
        }

        public MCPAgentBuilder WithStrategy(IAgentStrategy strategy)
        {
            _strategy = strategy;
            return this;
        }

        public MCPAgentBuilder WithMultiLLM(IMultiLLMProvider multiLLM)
        {
            _multiLLMProvider = multiLLM;
            return this;
        }

        public MCPAgentBuilder WithEvaluator(IEvaluator evaluator)
        {
            _evaluator = evaluator;
            return this;
        }

        public MCPAgentBuilder WithOptimizer(IOptimizer optimizer)
        {
            _optimizer = optimizer;
            return this;
        }

        public MCPAgentBuilder WithOptions(AgentOptions options)
        {
            _options = options;
            return this;
        }

        public MCPAgentBuilder WithHealthCheck(ILLMHealthCheck healthCheck)
        {
            _healthCheck = healthCheck;
            return this;
        }

        public MCPAgentBuilder WithQdrantOptions(QdrantOptions options)
        {
            _qdrantOptions = options;
            return this;
        }

        public IAgent Build()
        {
            // Verify LLM providers
            if (_multiLLMProvider == null && _llm == null)
            {
                throw new InvalidOperationException("Either LLM Provider or MultiLLM Provider must be provided.");
            }

            // If multiLLM is not provided, create a basic implementation that wraps the single LLM
            var multiLLM = _multiLLMProvider ?? new SingleLLMWrapper(_llm ??
                throw new InvalidOperationException("No valid LLM Provider available."));


            _tools ??= Array.Empty<IAgentTool>();
            _planner ??= new DefaultPlanner();
            _contextSource ??= new DefaultContextSource();
            _keyValueMemory ??= new InMemoryMemoryStore();
            _options ??= new AgentOptions();
            var embedding = new MultiLLMEmbeddingProvider([_llm]);
            _semanticMemory ??= new QdrantSemanticMemory(_qdrantOptions, embedding);
            // Create default health check if not provided
            _healthCheck ??= new DefaultLLMHealthCheck(
                multiLLM.GetProviders(),
                Microsoft.Extensions.Options.Options.Create(new HealthCheckOptions()));

            // Initialize remaining dependencies with multiLLM
            _optimizer ??= new DefaultOptimizer(multiLLM, _healthCheck);
            _postProcessor ??= new OptimizationPostProcessor(_optimizer);
            _strategy ??= new GoalDrivenStrategy();
            _evaluator ??= new DefaultEvaluator(multiLLM);

            return new MCPAgent(
                multiLLM,
                _tools,
                _planner,
                _contextSource,
                _keyValueMemory,
                _semanticMemory,
                _postProcessor,
                _strategy,
                multiLLM,
                _evaluator,
                _optimizer,
                _healthCheck,
                _options
            );
        }
    }
}
