using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions.Models
{
    /// <summary>
    /// Gói thông tin đầu vào cho agent để xử lý một goal
    /// </summary>
    public class AgentRequest
    {
        public string Goal { get; set; } = string.Empty;
        public AgentInputContext? InputContext { get; set; }
    }
}
