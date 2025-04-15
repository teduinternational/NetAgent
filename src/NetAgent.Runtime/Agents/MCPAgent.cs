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
using NetAgent.LLM.Preferences;
using System.Text;
using NetAgent.LLM.Providers;
using NetAgent.LLM.Monitoring;
using NetAgent.Core.Utils;

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
        private readonly IKeyValueMemoryStore _keyValueMemory;
        private readonly ISemanticMemoryStore _semanticMemory;
        private readonly IAgentPostProcessor _postProcessor;
        private readonly IAgentStrategy _strategy;
        private readonly IMultiLLMProvider _multiLLM;
        private readonly IEvaluator _evaluator;
        private readonly IOptimizer _optimizer;
        private readonly AgentOptions _options;
        private readonly ILLMPreferences? _llmPreferences;
        private readonly ILLMHealthCheck _healthCheck;

        public MCPAgent(
            ILLMProvider llm,
            IEnumerable<IAgentTool> tools,
            IAgentPlanner planner,
            IContextSource contextSource,
            IKeyValueMemoryStore keyValueMemoryStore,
            ISemanticMemoryStore semanticMemoryStore,
            IAgentPostProcessor postProcessor,
            IAgentStrategy strategy,
            IMultiLLMProvider multiLLM,
            IEvaluator evaluator,
            IOptimizer optimizer,
            ILLMHealthCheck healthCheck,
            AgentOptions options)
        {
            _llm = llm ?? throw new ArgumentNullException(nameof(llm));
            _tools = tools?.ToArray() ?? throw new ArgumentNullException(nameof(tools));
            _planner = planner ?? throw new ArgumentNullException(nameof(planner));
            _contextSource = contextSource ?? throw new ArgumentNullException(nameof(contextSource));
            _keyValueMemory = keyValueMemoryStore ?? throw new ArgumentNullException(nameof(keyValueMemoryStore));
            _postProcessor = postProcessor ?? throw new ArgumentNullException(nameof(postProcessor));
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _multiLLM = multiLLM ?? throw new ArgumentNullException(nameof(multiLLM));
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _optimizer = optimizer ?? throw new ArgumentNullException(nameof(optimizer));
            _healthCheck = healthCheck ?? throw new ArgumentNullException(nameof(healthCheck));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _semanticMemory = semanticMemoryStore;
            if (options.PreferredProviders?.Any() == true)
            {
                _llmPreferences = new LLMPreferences(options.PreferredProviders);
            }
        }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name => _options.Name ?? "MCPAgent";

        public async Task<AgentResponse> ProcessAsync(AgentRequest request)
        {
            request.InputContext = request.InputContext ?? new AgentInputContext()
            {
                Goal = request.Goal,
            };
            
            // Health check logic
            var healthResults = await _healthCheck.CheckAllProvidersAsync();

            var healthyProviders = healthResults
                .Where(r => r.Value.Status == HealthStatus.Healthy)
                .Select(r => r.Key)
                .ToList();

            if (!healthyProviders.Any())
            {
                throw new LLMException("No healthy LLM providers available");
            }

            // Get additional context from context source
            var additionalContext = await _contextSource.GetContextAsync();
            request.InputContext.Context += "\n" + additionalContext.Context;

            // Plan next steps using planner
            var plan = await _planner.PlanNextStepAsync(request.InputContext.Goal, request.InputContext);

            // Get strategy decision
            var decision = await _strategy.DecideNextStepAsync(request.InputContext.Goal, request.InputContext);

            // Create agent context
            var agentContext = new AgentContext
            {
                Goal = request.InputContext.Goal,
                Context = request.InputContext.Context,
                Plan = plan !=null ? plan.Goal : "No Plan",
                ToolOutput = decision != null ? decision.Plan : "No Plan",
            };

            var prompt = BuildPrompt(agentContext);

            // Optimize the prompt
            var optimizationResult = await _optimizer.OptimizeAsync(prompt.Content, request.InputContext.Goal, request.InputContext.Context);
            
            // Nếu tối ưu prompt bị lỗi, thử với provider khác
            if (optimizationResult.IsError && _multiLLM != null)
            {
                if (_llm != null && healthyProviders.Contains(_llm.Name))
                {
                    healthyProviders.Remove(_llm.Name);
                }

                if (healthyProviders.Any())
                {
                    // Tạo preferences mới với các provider còn lại
                    var fallbackPreferences = new LLMPreferences(healthyProviders);
                    var fallbackMultiLLM = new MultiLLMProvider(
                        _multiLLM.GetProviders(),
                        _multiLLM.GetScorer(),
                        _multiLLM.GetLogger(),
                        _healthCheck,
                        fallbackPreferences
                    );

                    // Thử tối ưu lại với provider khác
                    optimizationResult = await _optimizer.OptimizeAsync(prompt.Content, request.InputContext.Goal, request.InputContext.Context);
                }
            }

            prompt = new Prompt(optimizationResult.OptimizedPrompt);

            // Get relevant memories
            var relevantKeyValueMemories = await _keyValueMemory.RetrieveAsync(prompt.Content);
            if (!string.IsNullOrEmpty(relevantKeyValueMemories))
            {
                await _keyValueMemory.SaveAsync($"goal_{request.InputContext.Goal}", relevantKeyValueMemories);
                prompt.Content += "\nRelevant Context:\n" + relevantKeyValueMemories;
            }

            // Get relevant memories
            var relevantSemanticMemories = await _semanticMemory.SearchAsync(prompt.Content);
            if (relevantSemanticMemories.Count > 0)
            {
                await _semanticMemory.SaveAsync(1, prompt.Content);
                var resultString = PayloadHelper.ConvertSearchResultsToString(relevantSemanticMemories);

                prompt.Content += "\nRelevant Context:\n" + resultString;
            }
            else
            {
                await _semanticMemory.SaveAsync(1, prompt.Content);
            }

            // Create healthy preferences
            var healthyPreferences = new LLMPreferences(healthyProviders);

            // Generate response using LLM
            LLMResponse response;
            if (_multiLLM != null)
            {
                var multiLLMWithPreferences = new MultiLLMProvider(
                    _multiLLM.GetProviders(),
                    _multiLLM.GetScorer(),
                    _multiLLM.GetLogger(),
                    _healthCheck,
                    healthyPreferences
                );
                response = await multiLLMWithPreferences.GenerateAsync(prompt);
            }
            else
            {
                if (!healthyProviders.Contains(_llm.Name))
                {
                    throw new Exception($"LLM provider {_llm.Name} is not healthy");
                }
                response = await _llm.GenerateAsync(prompt);
            }

            // Evaluate response
            var evaluationResult = await _evaluator.EvaluateAsync(prompt.Content, response.Content, request.InputContext.Goal, request.InputContext.Context);
            
           if (evaluationResult.IsError)
            {
                // Critical error case - try with different provider
                if (_llm != null && healthyProviders.Contains(_llm.Name))
                {
                    healthyProviders.Remove(_llm.Name);
                }

                // If we still have healthy providers, try with a different one
                if (healthyProviders.Any() && _multiLLM != null)
                {
                    // Create new preferences with remaining healthy providers
                    var fallbackPreferences = new LLMPreferences(healthyProviders);
                    var fallbackMultiLLM = new MultiLLMProvider(
                        _multiLLM.GetProviders(),
                        _multiLLM.GetScorer(),
                        _multiLLM.GetLogger(),
                        _healthCheck,
                        fallbackPreferences
                    );

                    prompt.Content += "\nPrevious response had critical errors. Please fix based on this feedback: " + evaluationResult.Feedback;
                    response = await fallbackMultiLLM.GenerateAsync(prompt);
                }
                else
                {
                    // Log critical error when we can't retry
                    System.Diagnostics.Debug.WriteLine("Critical: Could not retry with different LLM provider - no healthy providers available");
                }
            }
            else if (!evaluationResult.IsAcceptable)
            {
                // Not acceptable but not critical error - try to improve with same provider
                prompt.Content += "\nPrevious response needs improvement based on this feedback: " + evaluationResult.Feedback;
                
                if (_multiLLM != null)
                {
                    response = await _multiLLM.GenerateAsync(prompt);
                }
                else
                {
                    response = await _llm.GenerateAsync(prompt);
                }
            }

            // Create and process response
            var result = new AgentResponse
            {
                Output = response.Content,
                FinalPrompt = prompt.Content,
                EvaluationScore = evaluationResult.Score,
                Plan = plan.ToString(),
                Decision = decision.ToString()
            };

            await _postProcessor.PostProcessAsync(result, request.InputContext);

            // Save to memory
            if (!string.IsNullOrEmpty(result.Output))
            {
                await _keyValueMemory.SaveAsync($"response_{request.InputContext.Goal}", result.Output);
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
