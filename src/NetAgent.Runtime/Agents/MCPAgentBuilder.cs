using NetAgent.Abstractions;
using NetAgent.Abstractions.Tools;
using NetAgent.Core.Contexts;
using NetAgent.Core.LLM;
using NetAgent.Core.Memory;
using NetAgent.Core.Planning;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Evaluation.SelfImproving;
using NetAgent.LLM.Interfaces;
using NetAgent.Optimization.Interfaces;
using NetAgent.Runtime.PostProcessing;
using NetAgent.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Runtime.Agents
{
    public class MCPAgentBuilder
    {
        private IContextSource? _contextSource;
        private IMemoryStore? _memory;
        private IAgentPlanner? _planner;
        private List<IAgentTool> _tools = new();
        private IAgentStrategy? _strategy;
        private IAgentPostProcessor? _postProcessor;
        private IEvaluator? _evaluator;
        private IOptimizer? _optimizer;
        private IMultiLLMProvider? _multiLLMProvider;

        public MCPAgentBuilder UseContextSource(IContextSource source)
        {
            _contextSource = source;
            return this;
        }

        public MCPAgentBuilder UseMemoryStore(IMemoryStore memory)
        {
            _memory = memory;
            return this;
        }

        public MCPAgentBuilder UsePlanner(IAgentPlanner planner)
        {
            _planner = planner;
            return this;
        }

        public MCPAgentBuilder UseTools(params IAgentTool[] tools)
        {
            _tools.AddRange(tools);
            return this;
        }

        public MCPAgentBuilder UseStrategy(IAgentStrategy strategy)
        {
            _strategy = strategy;
            return this;
        }

        public MCPAgentBuilder UsePostProcessor(IAgentPostProcessor processor)
        {
            _postProcessor = processor;
            return this;
        }

        public MCPAgentBuilder UseMultiLLMProvider(IMultiLLMProvider multiLLM)
        {
            _multiLLMProvider = multiLLM;
            return this;
        }

        public MCPAgentBuilder UseEvaluator(IEvaluator evaluator)
        {
            _evaluator = evaluator;
            return this;
        }

        public MCPAgentBuilder UseOptimizer(IOptimizer optimizer)
        {
            _optimizer = optimizer;
            return this;
        }

        public IAgent Build()
        {
            if (_contextSource is null) throw new InvalidOperationException("Context source is required.");
            if (_planner is null) throw new InvalidOperationException("Planner is required.");
            if (_memory is null) throw new InvalidOperationException("Memory store is required.");
            if (_strategy is null) throw new InvalidOperationException("Strategy is required.");
            if (_postProcessor is null) throw new InvalidOperationException("PostProcessor is required.");

            // ⚠️ Nếu dùng MultiLLM, wrap lại để chọn LLM tốt nhất dựa vào Evaluation
            ILLMProvider selectedLLM;

            if (_multiLLMProvider != null && _evaluator != null && _optimizer != null)
            {
                selectedLLM = new SelfImprovingLLMWrapper(
                    _multiLLMProvider, _evaluator, _optimizer
                );
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
