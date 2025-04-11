using NetAgent.Abstractions.Tools;
using System.Data;

namespace NetAgent.Tools.Standard
{
    public class CalculatorTool : IAgentTool
    {
        public string Name => "calculator";

        public Task<string> ExecuteAsync(string input)
        {
            try
            {
                var result = new DataTable().Compute(input, null)?.ToString();
                return Task.FromResult(result ?? "NaN");
            }
            catch
            {
                return Task.FromResult("Error evaluating expression");
            }
        }
    }
}
