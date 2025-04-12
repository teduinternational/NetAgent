using NetAgent.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Runtime.PostProcessing
{
    public class AgentPostProcessorPipeline : IAgentPostProcessor
    {
        private readonly IEnumerable<IAgentPostProcessor> _processors;

        public AgentPostProcessorPipeline(IEnumerable<IAgentPostProcessor> processors)
        {
            _processors = processors;
        }

        public async Task PostProcessAsync(AgentResponse result, AgentInputContext context)
        {
            foreach (var processor in _processors)
            {
                await processor.PostProcessAsync(result, context);
            }
        }
    }
}
