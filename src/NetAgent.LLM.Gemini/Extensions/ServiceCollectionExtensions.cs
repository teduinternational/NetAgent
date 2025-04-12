﻿using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.Gemini.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGeminiProvider(this IServiceCollection services, GeminiOptions options)
        {
            services.AddSingleton<ILLMProvider>(new GeminiLLMProvider(options));
            return services;
        }
    }
}
