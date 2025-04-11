using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.LLM.Interfaces
{
    public interface ILLMProvider
    {
        string Name { get; }

        /// <summary>
        /// Sinh một kết quả đầu ra duy nhất từ prompt.
        /// </summary>
        Task<string> GenerateAsync(string prompt);
    }
}
