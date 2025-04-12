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
using NetAgent.LLM.Preferences;
using System.Text;
using NetAgent.LLM.Providers;

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
        private readonly AgentOptions _options;
        private readonly ILLMPreferences? _llmPreferences;

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
            IOptimizer optimizer,
            AgentOptions options)
        {
            _llm = llm ?? throw new ArgumentNullException(nameof(llm));
            _tools = tools?.ToArray() ?? throw new ArgumentNullException(nameof(tools));
            _planner = planner ?? throw new ArgumentNullException(nameof(planner));
            _contextSource = contextSource ?? throw new ArgumentNullException(nameof(contextSource));
            _memory = memoryStore ?? throw new ArgumentNullException(nameof(memoryStore));
            _postProcessor = postProcessor ?? throw new ArgumentNullException(nameof(postProcessor));
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _multiLLM = multiLLM ?? throw new ArgumentNullException(nameof(multiLLM));
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _optimizer = optimizer ?? throw new ArgumentNullException(nameof(optimizer));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            
            if (options.PreferredProviders?.Any() == true)
            {
                _llmPreferences = new LLMPreferences(options.PreferredProviders);
            }
        }

        public string Name => _options.Name ?? "MCPAgent";
        public async Task<AgentResponse> ProcessAsync(AgentRequest request)
        {
            request.InputContext = request.InputContext ?? new AgentInputContext();

            // Tạo AgentContext từ AgentInputContext
            var agentContext = new AgentContext
            {
                Goal = request.InputContext.Goal,
                Context = request.InputContext.Context
            };

            var prompt = BuildPrompt(agentContext);
            var relevantMemories = await _memory.RetrieveAsync(prompt.Content);

            // Store memories directly in memory store if not null
            if (!string.IsNullOrEmpty(relevantMemories))
            {
                await _memory.SaveAsync($"goal_{request.Goal}", relevantMemories);
            }

            LLMResponse response;
            if (_multiLLM != null)
            {
                var multiLLMWithPreferences = new MultiLLMProvider(
                    _multiLLM.GetProviders(),
                    _multiLLM.GetScorer(),
                    _multiLLM.GetLogger(),
                    _llmPreferences
                );
                response = await multiLLMWithPreferences.GenerateAsync(prompt);
            }
            else
            {
                response = await _llm.GenerateAsync(prompt);
            }

            // Xử lý response bằng cách sử dụng IAgentPostProcessor
            var result = new AgentResponse
            {
                Output = response.Content,
                FinalPrompt = prompt.Content
            };
            await _postProcessor.PostProcessAsync(result, request.InputContext);

            // Lưu response đã xử lý vào memory store
            if (!string.IsNullOrEmpty(result.Output))
            {
                await _memory.SaveAsync($"response_{request.Goal}", result.Output);
            }

            return result;
        }

        private Prompt BuildPrompt(AgentContext context)
        {
            var sb = new StringBuilder();

            // Add system message if available
            if (!string.IsNullOrEmpty(_options.SystemMessage))
            {
                sb.AppendLine(_options.SystemMessage);
                sb.AppendLine();
            }

            // Add role information
            if (!string.IsNullOrEmpty(_options.Role))
            {
                sb.AppendLine($"Role: {_options.Role}");
                sb.AppendLine();
            }

            // Add goals if available
            if (_options.Goals?.Any() == true)
            {
                sb.AppendLine("Goals:");
                foreach (var goal in _options.Goals)
                {
                    sb.AppendLine($"- {goal}");
                }
                sb.AppendLine();
            }

            // Add enabled tools if any
            if (_options.EnabledTools?.Any() == true)
            {
                sb.AppendLine("Available Tools:");
                foreach (var tool in _options.EnabledTools)
                {
                    var toolInfo = _tools.FirstOrDefault(t => t.Name == tool);
                    if (toolInfo != null)
                    {
                        sb.AppendLine($"- {tool}: {toolInfo.Name}");
                    }
                }
                sb.AppendLine();
            }

            // Add current goal and context
            sb.AppendLine($"Current Goal: {context.Goal}");
            
            if (!string.IsNullOrEmpty(context.Context))
            {
                sb.AppendLine("\nContext:");
                sb.AppendLine(context.Context);
            }

            if (!string.IsNullOrEmpty(context.Plan))
            {
                sb.AppendLine("\nExecution Plan:");
                sb.AppendLine(context.Plan);
            }

            if (!string.IsNullOrEmpty(context.ToolOutput))
            {
                sb.AppendLine("\nTool Output:");
                sb.AppendLine(context.ToolOutput);
            }

            // Add language instruction if not English
            if (_options.Language != AgentLanguage.English)
            {
                sb.AppendLine($"\nPlease respond in {_options.Language}.");
            }

            sb.AppendLine("\nResponse:");
            return new Prompt()
            {
                Content = sb.ToString(),
            };
        }
    }
}
