using NetAgent.Abstractions;
using NetAgent.Abstractions.Models;
using NetAgent.Abstractions.Tools;
using NetAgent.Abstractions.LLM;
using NetAgent.Core.Contexts;
using NetAgent.Core.Memory;
using NetAgent.Core.Planning;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Evaluation.Models;
using NetAgent.Optimization.Interfaces;
using NetAgent.Runtime.PostProcessing;
using NetAgent.Strategy;

namespace NetAgent.Runtime.Agents
{
    /// <summary>
    /// Triển khai agent theo chuẩn MCP (Model Context Protocol)
    /// </summary>
    public class MCPAgent : IAgent
    {
        private readonly ILLMProvider _llm;
        private readonly IAgentTool[] _tools;
        private readonly IAgentPlanner _planner;
        private readonly IContextSource _contextSource;
        private readonly IMemoryStore _memory;
        private readonly IAgentPostProcessor _postProcessor;
        private readonly IAgentStrategy _strategy;
        private readonly IMultiLLMProvider _multiLLM;
        private readonly IEvaluator _evaluator;
        private readonly IOptimizer _optimizer;
        public MCPAgent(
            ILLMProvider llm,
            IEnumerable<IAgentTool> tools,
            IAgentPlanner planner,
            IContextSource contextSource,
            IMemoryStore memoryStore,
            IAgentPostProcessor postProcessor,
            IAgentStrategy strategy,
            IMultiLLMProvider multiLLM,
            IEvaluator evaluator,
            IOptimizer optimizer)
        {
            _llm = llm;
            _tools = tools.ToArray();
            _planner = planner;
            _contextSource = contextSource;
            _memory = memoryStore;
            _postProcessor = postProcessor;
            _strategy = strategy;
            _multiLLM = multiLLM;
            _evaluator = evaluator;
            _optimizer = optimizer;
        }

        public async Task<string> ExecuteGoalAsync(string goal)
        {
            // Bước 1: Lấy context đầu vào
            var inputContext = await _contextSource.GetContextAsync();

            // Bước 2: Chiến lược quyết định bước tiếp theo
            var decision = await _strategy.DecideNextStepAsync(goal, inputContext);

            // Bước 3: Lập kế hoạch dựa trên strategy
            var plan = await _planner.PlanNextStepAsync(goal, inputContext);

            // Bước 4: Dùng tool nếu strategy yêu cầu
            string toolOutput = string.Empty;
            if (!decision.SkipToolExecution && decision.ToolToUse is not null)
            {
                var selectedTool = _tools.FirstOrDefault(t => t.Name == decision.ToolToUse);
                if (selectedTool is not null)
                {
                    toolOutput = await selectedTool.ExecuteAsync(decision.Plan ?? plan);
                }
            }

            // Bước 5: Build context cho LLM
            var agentContext = new AgentContext
            {
                Goal = goal,
                Plan = plan,
                Context = inputContext.Context,
                ToolOutput = toolOutput
            };

            // Bước 6: Tạo prompt
            var finalPrompt = BuildPrompt(agentContext);

            // Bước 7: Gọi MultiLLMProvider để generate nhiều response
            var responses = await _multiLLM.GenerateFromAllAsync(finalPrompt);

            // Bước 8: Evaluate tất cả response
            var evaluated = new List<(string response, EvaluationResult result)>();
            foreach (var r in responses)
            {
                var result = await _evaluator.EvaluateAsync(finalPrompt, r, goal, inputContext.Context);
                evaluated.Add((r, result));
            }

            // Bước 9: Chọn best response
            var best = evaluated.OrderByDescending(e => e.result.Score).First();

            // Bước 10: Nếu score thấp thì optimize lại prompt
            string finalResponse = best.response;
            if (best.result.Score < 0.7)
            {
                var optimizedPrompt = await _optimizer.OptimizeAsync(finalPrompt, goal, inputContext.Context);
                finalResponse = await _llm.GenerateAsync(optimizedPrompt.OptimizedPrompt);
            }

            // Bước 11: Lưu vào memory
            await _memory.SaveAsync(goal, finalResponse);

            // Bước 12: Post-process
            await _postProcessor.PostProcessAsync(
                new AgentResult { Output = finalResponse, FinalPrompt = finalPrompt },
                inputContext);

            return finalResponse;
        }

        private string BuildPrompt(AgentContext context)
        {
            return $"""
                    Goal: {context.Goal}
                    Context: {context.Context}
                    Plan: {context.Plan}
                    Tool Result: {context.ToolOutput}
                    Answer:
                    """;
        }
    }
}
