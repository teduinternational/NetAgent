using Microsoft.Extensions.DependencyInjection;
using NetAgent.Core.Memory;
using NetAgent.Memory.SemanticQdrant.Models;
using Microsoft.Extensions.Configuration;


namespace NetAgent.Memory.SemanticQdrant.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQdrantSemanticMemory(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection("NetAgent:Memory:Qdrant");
            var options = section.Get<QdrantOptions>() ?? new QdrantOptions();
            services.AddSingleton(options);
            services.AddSingleton<ISemanticMemoryStore, QdrantSemanticMemory>();
            services.AddHttpClient<QdrantSemanticMemory>();
            return services;
        }
    }
}
