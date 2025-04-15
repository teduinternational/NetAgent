using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions.Models
{
    public class SemanticSearchResult
    {
        public string Id { get; set; }           // ID vector
        public float Score { get; set; }         // Điểm tương đồng
        public Dictionary<string, object> Payload { get; set; } // Metadata

        public string ToMemoryString()
        {
            var parts = Payload.Select(kv => $"{kv.Key}: {kv.Value}");
            return string.Join(" | ", parts); // dạng compact
        }
    }
}
