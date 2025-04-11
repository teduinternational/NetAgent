using NetAgent.Evaluation.Interfaces;
using NetAgent.Optimization.Interfaces;
using NetAgent.Abstractions.LLM;

namespace NetAgent.Evaluation.SelfImproving
{
    public class SelfImprovingLLMWrapper : IMultiLLMProvider
    {
        private readonly IMultiLLMProvider _multiLLM;
        private readonly IEvaluator _evaluator;
        private readonly IOptimizer _optimizer;

        public SelfImprovingLLMWrapper(IMultiLLMProvider multiLLM, IEvaluator evaluator, IOptimizer optimizer)
        {
            _multiLLM = multiLLM;
            _evaluator = evaluator;
            _optimizer = optimizer;
        }

        public string Name => "SelfImprovingLLMWrapper";

        public async Task<string[]> GenerateFromAllAsync(string prompt)
        {
            return await _multiLLM.GenerateFromAllAsync(prompt);
        }

        public async Task<string> GenerateBestAsync(string prompt)
        {
            var candidates = await _multiLLM.GenerateFromAllAsync(prompt);

            var scored = new List<(string Output, double Score)>();

            foreach (var output in candidates)
            {
                var result = await _evaluator.EvaluateAsync(prompt, output, goal: string.Empty, context: string.Empty);
                scored.Add((output, result.Score));
            }

            var best = scored.OrderByDescending(s => s.Score).FirstOrDefault();
            return best.Output ?? string.Empty;
        }

        public async Task<string> GenerateAsync(string prompt, string goal = "", string context = "")
        {
            // 1. Tối ưu prompt trước khi gửi
            var optimized = await _optimizer.OptimizeAsync(prompt, goal, context);
            var optimizedPrompt = optimized.OptimizedPrompt;

            // 2. Gửi đến tất cả LLMs
            var outputs = await _multiLLM.GenerateFromAllAsync(optimizedPrompt);
            if (!outputs.Any())
            {
                return string.Empty;
            }

            // 3. Đánh giá song song các kết quả
            var evaluationTasks = outputs.Select(async output =>
            {
                try
                {
                    var eval = await _evaluator.EvaluateAsync(optimizedPrompt, output, goal, context);
                    return (Output: output, Score: eval.Score);
                }
                catch
                {
                    return (Output: output, Score: 0.0);
                }
            });

            var evaluations = await Task.WhenAll(evaluationTasks);
            
            // 4. Chọn kết quả có score cao nhất
            var best = evaluations.OrderByDescending(e => e.Score).FirstOrDefault();
            return best.Output;
        }
    }
}
