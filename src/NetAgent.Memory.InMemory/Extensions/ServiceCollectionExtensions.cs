using Microsoft.Extensions.DependencyInjection;
using NetAgent.Core.Memory;

namespace NetAgent.Memory.InMemory.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryMemoryStore(this IServiceCollection services)
        {
            services.AddSingleton<IKeyValueMemoryStore, InMemoryMemoryStore>();
            return services;
        }
    }
}
