using NetAgent.Abstractions;
using NetAgent.Abstractions.Tools;
using NetAgent.Abstractions.LLM;
using NetAgent.Core.Contexts;
using NetAgent.Core.Memory;
using NetAgent.Core.Planning;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Optimization.Interfaces;
using NetAgent.Runtime.PostProcessing;
using NetAgent.Strategy;
using NetAgent.Planner.Default;
using NetAgent.Memory.InMemory;
using NetAgent.Runtime.Optimization;
using NetAgent.Optimization.Optimizers;
using NetAgent.Evaluation.Evaluators;
using NetAgent.Strategy.Strategies;
using NetAgent.LLM.Providers;

namespace NetAgent.Runtime.Agents
{
    public class MCPAgentBuilder
    {
        private ILLMProvider? _llm;
        private IEnumerable<IAgentTool>? _tools;
        private IAgentPlanner? _planner;
        private IContextSource? _contextSource;
        private IMemoryStore? _memory;
        private IAgentPostProcessor? _postProcessor;
        private IAgentStrategy? _strategy;
        private IMultiLLMProvider? _multiLLMProvider;
        private IEvaluator? _evaluator;
        private IOptimizer? _optimizer;

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

        public MCPAgentBuilder WithMemory(IMemoryStore memory)
        {
            _memory = memory;
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

        public IAgent Build()
        {
            _tools ??= Array.Empty<IAgentTool>();
            _planner ??= new DefaultPlanner();
            _contextSource ??= new DefaultContextSource();
            _memory ??= new InMemoryMemoryStore();
            _postProcessor ??= new OptimizationPostProcessor(_optimizer);
            _strategy ??= new GoalDrivenStrategy();
            _evaluator ??= new LLMEvaluator(_llm);
            _optimizer ??= new PromptOptimizer(_multiLLMProvider);

            ILLMProvider selectedLLM;
            if (_llm != null)
            {
                selectedLLM = _llm;
            }
            else if (_multiLLMProvider != null)
            {
                selectedLLM = _multiLLMProvider; // fallback to default inside
            }
            else
            {
                throw new InvalidOperationException("LLM Provider is missing.");
            }

            return new MCPAgent(
                selectedLLM,
                _tools,
                _planner,
                _contextSource,
                _memory,
                _postProcessor,
                _strategy,
                _multiLLMProvider,
                _evaluator,
                _optimizer
            );
        }
    }
}
