﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using NetAgent.Hosting.Extensions;
using NetAgent.Abstractions.Models;
using Microsoft.Extensions.Configuration;
using NetAgent.Abstractions;
using NetAgent.LLM.Extensions;
using NetAgent.LLM.Factory;
using NetAgent.Abstractions.LLM;
using NetAgent.LLM.OpenAI;
using NetAgent.LLM.Claude;
using NetAgent.LLM.DeepSeek;
using NetAgent.LLM.Monitoring;
using NetAgent.LLM.RateLimiting;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Read configuration from appsettings.json
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        // Add NetAgent services with configuration
        builder.Services.AddNetAgent(builder.Configuration);

        // Add LLM Caching
        builder.Services.AddLLMCaching(options =>
        {
            options.DefaultExpiration = TimeSpan.FromHours(24); // Cache responses for 24 hours
            options.MaxCacheSize = 1000; // Store up to 1000 responses
        });

        // Configure rate limiting with proper DI
        builder.Services.AddLLMRateLimiting(options =>
        {
            options.RequestsPerMinute = 60;
            options.TokensPerMinute = 100000;
            options.ConcurrentRequests = 5;
            options.EnableAdaptiveThrottling = true;
            options.RetryAfter = TimeSpan.FromSeconds(1);
        });

        // Configure monitoring
        builder.Services.Configure<LLMMetricsOptions>(options => 
        {
            options.EnableLatencyTracking = true;
            options.EnableTokenCounting = true;
            options.EnableErrorTracking = true;
            options.EnableResponseTracking = true;
        });

        builder.Services.Configure<HealthCheckOptions>(options =>
        {
            options.Timeout = TimeSpan.FromSeconds(10);
            options.FailureThresholdEnabled = true;
            options.FailureThreshold = 3;
            options.FailureWindow = TimeSpan.FromMinutes(5);
        });

        // Register monitoring services
        builder.Services.AddSingleton<ILLMMetricsCollector, DefaultLLMMetricsCollector>();
        builder.Services.AddSingleton<ILLMHealthCheck, DefaultLLMHealthCheck>();

        // Configure LLM providers
        builder.Services.AddSingleton<ILLMProvider>(sp =>
        {
            var factory = LLMProviderFactory.Create(new LLMFactoryOptions
            {
                Provider = LLMProviderType.OpenAI,
                OpenAI = new OpenAIOptions { Model = "gpt-4" }
            });
            
            var metrics = sp.GetRequiredService<ILLMMetricsCollector>();
            var monitoredProvider = new MonitoringLLMProviderDecorator(factory, metrics);
            // Rate limiting will be automatically applied through DI
            return monitoredProvider;
        });

        builder.Services.AddSingleton<ILLMProvider>(sp =>
        {
            var factory = LLMProviderFactory.Create(new LLMFactoryOptions
            {
                Provider = LLMProviderType.Claude,
                Claude = new ClaudeLLMOptions { Model = "claude-3-opus-20240229" }
            });
            
            var metrics = sp.GetRequiredService<ILLMMetricsCollector>();
            var monitoredProvider = new MonitoringLLMProviderDecorator(factory, metrics);
            // Rate limiting will be automatically applied through DI
            return monitoredProvider;
        });

        builder.Services.AddSingleton<ILLMProvider>(sp =>
        {
            var factory = LLMProviderFactory.Create(new LLMFactoryOptions
            {
                Provider = LLMProviderType.DeepSeek,
                DeepSeek = new DeepSeekOptions { Model = "deepseek-chat" }
            });
            
            var metrics = sp.GetRequiredService<ILLMMetricsCollector>();
            var monitoredProvider = new MonitoringLLMProviderDecorator(factory, metrics);
            // Rate limiting will be automatically applied through DI
            return monitoredProvider;
        });

        builder.Services.AddMultiLLMProviders(); // Add MultiLLMProvider support

        var host = builder.Build();
        var serviceProvider = host.Services;

        // Add health check monitoring
        var healthCheck = serviceProvider.GetRequiredService<ILLMHealthCheck>();
        
        // Run initial health check and cache results
        var healthResults = await healthCheck.CheckAllProvidersAsync();
        
        var hasHealthyProvider = false;
        foreach (var (provider, result) in healthResults)
        {
            Console.WriteLine($"Provider {provider} health status: {result.Status}");
            if (result.Status != HealthStatus.Healthy)
            {
                Console.WriteLine($"Message: {result.Message}");
            }
            else
            {
                hasHealthyProvider = true;
            }
        }

        if (!hasHealthyProvider)
        {
            Console.WriteLine("Error: No healthy LLM providers available. Please check your configuration and try again.");
            return; // Exit if no providers are healthy
        }

        // Create agents using registered MultiLLMProvider
        var agentFactory = serviceProvider.GetRequiredService<IAgentFactory>();

        var developerAgent = await agentFactory.CreateAgent(new AgentOptions
        {
            Name = "Developer",
            Role = "You are a senior developer with expertise in Azure and authentication systems. You focus on technical implementation details and best practices.",
            Goals = new[] { "Understand technical requirements", "Identify potential technical challenges", "Suggest implementation approach" },
            Temperature = 0.7f,
            MaxTokens = 2000,
            Model = "gpt-4",
            SystemMessage = "You are an expert developer focusing on Azure solutions and best practices. Provide detailed technical insights and implementation strategies.",
            Memory = new MemoryOptions
            {
                MaxTokens = 4000,
                RelevanceThreshold = 0.7f
            },
            PreferredProviders = new[] { "Claude" }, // Specify preferred providers for developer agent
        });

        var productOwnerAgent = await agentFactory.CreateAgent(new AgentOptions
        {
            Name = "Product Owner",
            Role = "You are a product owner who focuses on business value and user experience. You represent stakeholder interests and ensure requirements are clear.",
            Goals = new[] { "Clarify business requirements", "Define acceptance criteria", "Ensure user value" },
            Temperature = 0.8f,
            MaxTokens = 1500,
            Model = "gpt-4",
            SystemMessage = "You are a product owner focused on delivering business value. Consider user needs and stakeholder requirements in your responses.",
            Memory = new MemoryOptions
            {
                MaxTokens = 3000,
                RelevanceThreshold = 0.8f
            },
            PreferredProviders = new[] { "Claude" }, // Product Owner prefers Claude
        });

        var scrumMasterAgent = await agentFactory.CreateAgent(new AgentOptions
        {
            Name = "Scrum Master",
            Role = "You are a scrum master who facilitates the discussion, ensures clarity, and helps identify blockers and dependencies.",
            Goals = new[] { "Facilitate discussion", "Ensure clear requirements", "Identify blockers and risks" },
            Temperature = 0.6f,
            MaxTokens = 1500,
            Model = "gpt-4",
            SystemMessage = "You are a scrum master responsible for facilitating effective communication and identifying project risks. Keep discussions focused and productive.",
            Memory = new MemoryOptions
            {
                MaxTokens = 3000,
                RelevanceThreshold = 0.75f
            },
            PreferredProviders = new[] { "Claude" }, // Scrum Master can use DeepSeek or OpenAI
        });

        // Start the discussion about Azure SSO authentication story
        var discussion = new StringBuilder();
        discussion.AppendLine("User Story Discussion: Azure SSO Authentication\n");

        // PO starts by presenting the story
        var poResponse = await productOwnerAgent.ProcessAsync(new AgentRequest()
        {
            Goal = "Present the user story for Azure SSO authentication, including business value and acceptance criteria.",
        });
        discussion.AppendLine($"Product Owner: {poResponse}\n");

        // Developer asks clarifying technical questions
        var devResponse = await developerAgent.ProcessAsync(new AgentRequest()
        {
            Goal = $"Based on the PO's story: {poResponse}\nAsk specific technical questions about the Azure SSO implementation requirements."
        });
        discussion.AppendLine($"Developer: {devResponse}\n");

        // PO responds to technical questions
        var poFollowup = await productOwnerAgent.ProcessAsync(new AgentRequest()
        {
            Goal = $"Address the developer's questions: {devResponse}"
        });
        discussion.AppendLine($"Product Owner: {poFollowup}\n");

        // Scrum Master facilitates and summarizes
        var smResponse = await scrumMasterAgent.ProcessAsync(new AgentRequest()
        {
            Goal = $"Based on the discussion so far:\n{discussion}\nSummarize the key points, identify any gaps or risks, and suggest next steps."
        });
        discussion.AppendLine($"Scrum Master: {smResponse}\n");

        // Final technical assessment from developer
        var devFinal = await developerAgent.ProcessAsync(new AgentRequest()
        {
            Goal = $"Based on the full discussion:\n{discussion}\nProvide a final technical assessment and confirm if we have enough information to start implementation."
        });
        discussion.AppendLine($"Developer: {devFinal}\n");

        // Final user story compilation by Product Owner for Jira
        var finalUserStory = await productOwnerAgent.ProcessAsync(new AgentRequest()
        {
            Goal = $"Compile the final user story for Jira based on the discussion:\n{discussion}\nEnsure it includes business value, acceptance criteria, and any technical considerations."
        });
        discussion.AppendLine("\nFinal User Story for Jira:");
        discussion.AppendLine(finalUserStory.Output);

        // Print the full discussion
        Console.WriteLine(discussion.ToString());

        await host.RunAsync();
    }
}