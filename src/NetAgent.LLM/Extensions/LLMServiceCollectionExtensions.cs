using Microsoft.Extensions.DependencyInjection;
using NetAgent.LLM.Interfaces;
using NetAgent.LLM.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.LLM.Extensions
{
    public static class LLMServiceCollectionExtensions
    {
        public static IServiceCollection AddMultiLLMProvider(this IServiceCollection services)
        {
            services.AddSingleton<IMultiLLMProvider, MultiLLMProvider>();
            return services;
        }
    }
}
