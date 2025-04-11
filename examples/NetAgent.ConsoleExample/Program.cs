using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions;
using NetAgent.Hosting.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using NetAgent.Runtime.Extensions;
using NetAgent.LLM.Extensions;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddPostProcessors(context.Configuration);
        services.AddMultiLLMProvider();
        services.AddAgentToolsFromConfig(context.Configuration);
    });

var host = builder.Build();
var agent = host.Services.GetRequiredService<IAgent>();

Console.Write("Goal: ");
var goal = Console.ReadLine() ?? "";
var result = await agent.ExecuteGoalAsync(goal);
Console.WriteLine($"Agent: {result}");