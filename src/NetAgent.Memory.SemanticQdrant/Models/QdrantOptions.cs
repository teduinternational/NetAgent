using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Memory.SemanticQdrant.Models
{
    public class QdrantOptions
    {
        public string Endpoint { get; set; } = "http://localhost:6333";
        public string CollectionName { get; set; } = "agent_memory";
        public int Dimension { get; set; } = 1536;
    }

}
