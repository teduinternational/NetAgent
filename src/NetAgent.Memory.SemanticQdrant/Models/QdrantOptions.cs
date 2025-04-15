using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Memory.SemanticQdrant.Models
{
    public class QdrantOptions
    {
        public string ApiKey { get; set; }
        public string Endpoint { get; set; }
        public string CollectionName { get; set; } = "agent_memory";
        public int Dimension { get; set; } = 1536;
    }

}
