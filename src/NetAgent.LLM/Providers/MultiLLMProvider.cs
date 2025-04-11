using NetAgent.LLM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.LLM.Providers
{
    public class MultiLLMProvider : IMultiLLMProvider
    {
        private readonly IEnumerable<ILLMProvider> _providers;

        public MultiLLMProvider(IEnumerable<ILLMProvider> providers)
        {
            _providers = providers;
        }

        public string Name => "MultiLLM";

        /// <summary>
        /// Sinh đầu ra từ tất cả các LLM được cấu hình.
        /// </summary>
        public async Task<string[]> GenerateFromAllAsync(string prompt)
        {
            var tasks = _providers.Select(p => p.GenerateAsync(prompt));
            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Sinh đầu ra tốt nhất theo logic đơn giản (ví dụ: dài nhất).
        /// </summary>
        public async Task<string> GenerateBestAsync(string prompt)
        {
            var results = await GenerateFromAllAsync(prompt);

            // TODO: có thể thay bằng gọi IEvaluator trong SelfImproving wrapper
            return results.OrderByDescending(r => r.Length).FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Default behavior cho GenerateAsync: gọi GenerateBestAsync.
        /// </summary>
        public Task<string> GenerateAsync(string prompt)
        {
            return GenerateBestAsync(prompt);
        }
    }
}
