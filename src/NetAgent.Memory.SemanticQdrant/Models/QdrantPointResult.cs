using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Memory.SemanticQdrant.Models
{
    public class QdrantPointResult
    {
        public float score { get; set; }
        public Dictionary<string, object> payload { get; set; } = new();
    }
}
