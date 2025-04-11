using NetAgent.Abstractions.Models;
using NetAgent.Core.Planning;

namespace NetAgent.Planner.Default
{
    public class DefaultPlanner : IAgentPlanner
    {
        public Task<Plan> PlanNextStepAsync(string goal, AgentInputContext context)
        {
            // Dummy planning logic (real logic should call LLM or rule-based engine)
            var steps = new List<PlanStep>
            {
                new PlanStep
                {
                    Description = $"Phân tích mục tiêu: {goal}",
                    ToolToUse = "AnalyzerTool",
                    Input = goal
                },
                new PlanStep
                {
                    Description = $"Xử lý dữ liệu đầu vào",
                    ToolToUse = "DataTool",
                    Input = context.Goal
                },
                new PlanStep
                {
                    Description = "Tổng hợp kết quả",
                    ToolToUse = "",
                    Input = "",
                    IsFinalStep = true
                }
            };

            return Task.FromResult(new Plan { Steps = steps });
        }
    }
}
