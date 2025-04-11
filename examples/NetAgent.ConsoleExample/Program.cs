﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using NetAgent.Hosting.Extensions;
using NetAgent.Abstractions.Models;
using Microsoft.Extensions.Configuration;
using NetAgent.Abstractions;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        // Đọc configuration từ appsettings.json
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        // Thêm NetAgent services với configuration
        builder.Services.AddNetAgentFromConfig(builder.Configuration);

        var host = builder.Build();
        var serviceProvider = host.Services;

        // Create three agents with different roles
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
            }
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
            }
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
            }
        });

        // Start the discussion about Azure SSO authentication story
        var discussion = new StringBuilder();
        discussion.AppendLine("User Story Discussion: Azure SSO Authentication\n");

        // PO starts by presenting the story
        var poResponse = await productOwnerAgent.ExecuteGoalAsync("Present the Azure SSO authentication user story from a business perspective, including the main requirements.");
        discussion.AppendLine($"Product Owner: {poResponse}\n");

        // Developer asks clarifying technical questions
        var devResponse = await developerAgent.ExecuteGoalAsync($"Based on the PO's story: {poResponse}\nAsk specific technical questions about the Azure SSO implementation requirements.");
        discussion.AppendLine($"Developer: {devResponse}\n");

        // PO responds to technical questions
        var poFollowup = await productOwnerAgent.ExecuteGoalAsync($"Address the developer's questions: {devResponse}");
        discussion.AppendLine($"Product Owner: {poFollowup}\n");

        // Scrum Master facilitates and summarizes
        var smResponse = await scrumMasterAgent.ExecuteGoalAsync($"Based on the discussion so far:\n{discussion}\nSummarize the key points, identify any gaps or risks, and suggest next steps.");
        discussion.AppendLine($"Scrum Master: {smResponse}\n");

        // Final technical assessment from developer
        var devFinal = await developerAgent.ExecuteGoalAsync($"Based on the full discussion:\n{discussion}\nProvide a final technical assessment and confirm if we have enough information to start implementation.");
        discussion.AppendLine($"Developer: {devFinal}\n");

        // Final user story compilation by Product Owner for Jira
        var finalUserStory = await productOwnerAgent.ExecuteGoalAsync($"Based on the complete discussion:\n{discussion}\nCompile a final user story summary for Jira including:\n- Description\n- Acceptance Criteria\n- Technical Considerations from the discussion\n- Dependencies identified\n- Definition of Done");
        discussion.AppendLine("\nFinal User Story for Jira:");
        discussion.AppendLine(finalUserStory);

        // Print the full discussion
        Console.WriteLine(discussion.ToString());

        await host.RunAsync();
    }
}