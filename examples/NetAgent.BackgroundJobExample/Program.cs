using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetAgent.BackgroundJobExample;
using NetAgent.Hosting.Extensions;
using NetAgent.LLM.Extensions;
using NetAgent.Runtime.Extensions;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json");
    })
    .ConfigureServices((context, services) =>
    {
        services.AddPostProcessors(context.Configuration);
        services.AddMultiLLMProviders();
        services.AddAgentTools(context.Configuration);
        services.AddHostedService<AgentHostedService>();
    });

await builder.Build().RunAsync();