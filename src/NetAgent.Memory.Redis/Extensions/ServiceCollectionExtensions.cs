using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetAgent.Core.Memory;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Memory.Redis.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisMemoryStore(this IServiceCollection services, IConfiguration config)
        {
            var redisConnectionString = config.GetConnectionString("Redis") ?? "localhost:6379";
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<IKeyValueMemoryStore, RedisMemoryStore>();
            return services;
        }
    }
}
