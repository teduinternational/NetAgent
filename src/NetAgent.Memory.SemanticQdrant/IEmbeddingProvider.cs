using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Memory.SemanticQdrant
{
    public interface IEmbeddingProvider
    {
        Task<float[]> GetEmbeddingAsync(string input);
    }

}
