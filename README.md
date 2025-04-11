# NetAgent

A powerful and flexible .NET framework for building AI agents with support for multiple LLM providers. NetAgent provides a modular architecture that allows you to easily integrate AI capabilities into your .NET applications.

## Table of Contents
- [Solution Structure](#solution-structure)
- [Features](#features)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Extending the Framework](#extending-the-framework)
- [Error Handling](#error-handling)
- [Logging](#logging)
- [Examples](#examples)
- [Contributing](#contributing)
- [License](#license)

## Solution Structure

The solution consists of several key projects:

### Core Projects
- **NetAgent.Abstractions**: Contains interfaces and base models for the framework
- **NetAgent.Core**: Core implementation of the agent system
- **NetAgent.Runtime**: Runtime execution engine for agents

### LLM Integration
- **NetAgent.LLM**: Base LLM integration interfaces
- **NetAgent.LLM.OpenAI**: OpenAI provider implementation
- **NetAgent.LLM.AzureOpenAI**: Azure OpenAI provider
- **NetAgent.LLM.Ollama**: Ollama provider for local LLM deployment
- **NetAgent.LLM.Factory**: Factory pattern for LLM provider initialization

### Memory Management
- **NetAgent.Memory.InMemory**: In-memory storage implementation
- **NetAgent.Memory.Redis**: Redis-based persistent storage

### Planning & Optimization
- **NetAgent.Planner.Default**: Default planning implementation
- **NetAgent.Planner.CustomRules**: Custom planning rules engine
- **NetAgent.Optimization**: Optimization strategies for agent performance
- **NetAgent.Strategy**: Strategic decision making components

### Tools & Evaluation
- **NetAgent.Tools.Standard**: Standard tool collection
- **NetAgent.Evaluation**: Evaluation and metrics collection

## Features

- **Multiple LLM Providers**:
  - OpenAI (GPT-3.5, GPT-4)
  - Azure OpenAI Service
  - Ollama (Local LLM deployment)
  
- **Memory Systems**:
  - In-memory storage
  - Redis persistence
  - Extensible memory provider interface
  
- **Advanced Planning**:
  - Default planner with optimization
  - Custom rules engine
  - Strategic decision making
  
- **Built-in Tools**:
  - DateTime handling
  - Weather information
  - HTTP requests
  - Calculator
  - Web search
  
- **Evaluation & Optimization**:
  - LLM-based evaluation
  - Prompt optimization
  - Performance metrics

## Installation

```bash
# Install the main package
dotnet add package NetAgent

# Optional packages based on your needs
dotnet add package NetAgent.LLM.OpenAI
dotnet add package NetAgent.Memory.Redis
```

## Configuration

### Basic Configuration (appsettings.json)

```json
{
  "NetAgent": {
    "PostProcessors": {
      "EnableLogging": true,
      "EnableOptimization": true
    },
    "LLM": {
      "Provider": "openai",
      "OpenAI": {
        "ApiKey": "your-api-key",
        "Model": "gpt-3.5-turbo",
        "Temperature": 0.7,
        "MaxTokens": 2000
      }
    },
    "Tools": {
      "Types": [ "datetime", "weather", "http" ],
      "EnableAll": false,
      "CustomToolsNamespace": "YourNamespace.Tools"
    },
    "Memory": {
      "Type": "inmemory",
      "MaxItems": 1000
    }
  }
}
```

### Advanced Configuration

```json
{
  "NetAgent": {
    "PostProcessors": {
      "EnableLogging": true,
      "EnableOptimization": true
    },
    "Optimizer": {
      "Type": "prompt"
    },
    "Evaluator": {
      "Type": "llm"
    },
    "LLM": {
      "Provider": "azureopenai",
      "AzureOpenAI": {
        "Endpoint": "your-endpoint",
        "ApiKey": "your-api-key",
        "DeploymentName": "your-deployment"
      }
    },
    "Planner": {
      "Type": "customrules"
    },
    "Tools": {
      "Types": [ "datetime", "weather", "http", "calculator", "websearch" ]
    },
    "Context": {
      "Type": "default"
    },
    "Memory": {
      "Type": "redis",
      "Redis": {
        "ConnectionString": "localhost:6379"
      }
    }
  }
}
```

## Usage

### Basic Integration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add NetAgent services
builder.Services.AddNetAgentFromConfig(builder.Configuration);

// -or- Configure manually with fluent API
builder.Services.AddNetAgent(options => {
    options.UseLLM(LLMProviderType.OpenAI, config => {
        config.ApiKey = "your-api-key";
        config.Model = "gpt-4";
    })
    .UseMemory(MemoryType.InMemory, config => {
        config.MaxItems = 1000;
    })
    .UseTools(builder => {
        builder.AddStandardTools()
               .AddCustomTool<MyCustomTool>();
    })
    .UseEvaluation(config => {
        config.EnableMetrics = true;
        config.CollectPerformanceData = true;
    });
});
```

### Advanced Usage with Context and Memory

```csharp
public class AgentService
{
    private readonly IAgent _agent;
    private readonly ILogger<AgentService> _logger;
    private readonly IMemoryStore _memoryStore;

    public AgentService(
        IAgent agent, 
        ILogger<AgentService> logger,
        IMemoryStore memoryStore)
    {
        _agent = agent;
        _logger = logger;
        _memoryStore = memoryStore;
    }

    public async Task<AgentResponse> ProcessRequestAsync(
        string input, 
        Dictionary<string, object> contextData = null)
    {
        try
        {
            var request = new AgentRequest
            {
                Input = input,
                Context = new AgentContext
                {
                    Data = contextData ?? new Dictionary<string, object>(),
                    Locale = "en-US",
                    TimeZone = "UTC",
                    MaxTokens = 2000
                }
            };

            // Load relevant memory
            var memories = await _memoryStore.LoadRelevantAsync(input);
            request.Context.Memories = memories;

            var response = await _agent.ProcessAsync(request);

            // Store the interaction in memory
            await _memoryStore.SaveAsync(new Memory
            {
                Input = input,
                Output = response.Output,
                Timestamp = DateTime.UtcNow,
                Metadata = response.Metadata
            });

            return response;
        }
        catch (NetAgentException ex)
        {
            _logger.LogError(ex, "Agent processing failed");
            throw;
        }
    }
}
```

## Extending the Framework

### Custom Tool Implementation

```csharp
public class CustomApiTool : IToolPlugin
{
    private readonly HttpClient _client;
    
    public string Name => "customapi";

    public CustomApiTool(HttpClient client)
    {
        _client = client;
    }

    public async Task<string> ExecuteAsync(string input)
    {
        // Implement your custom API logic
        var response = await _client.GetAsync($"api/endpoint?query={input}");
        return await response.Content.ReadAsStringAsync();
    }
}

// Registration
services.AddSingleton<IToolPlugin, CustomApiTool>();
```

### Custom Memory Provider

```csharp
public class CustomMemoryStore : IMemoryStore
{
    public async Task SaveAsync(Memory memory)
    {
        // Custom save implementation
    }

    public async Task<IEnumerable<Memory>> LoadAsync(int limit = 10)
    {
        // Custom load implementation
        return await Task.FromResult(new List<Memory>());
    }

    public async Task ClearAsync()
    {
        // Custom clear implementation
    }
}

// Registration
services.AddSingleton<IMemoryStore, CustomMemoryStore>();
```

## Examples

You can find complete working examples in the `examples` directory:

### Console Application
See `NetAgent.ConsoleExample` for a simple console-based agent implementation.

### Web API
Check `NetAgent.WebExample` for a RESTful API implementation using ASP.NET Core.

### Background Service
Explore `NetAgent.BackgroundJobExample` for implementing agents as background services.

## Error Handling

The framework provides several exception types for specific scenarios:

- `NetAgentException`: Base exception for all framework errors
- `ConfigurationException`: Configuration validation and loading errors
- `ProviderException`: LLM provider specific errors
- `ToolException`: Tool execution errors
- `PlanningException`: Planning and execution strategy errors

Example error handling:

```csharp
try
{
    var response = await _agent.ProcessAsync(request);
}
catch (ConfigurationException ex)
{
    _logger.LogError("Configuration error: {Message}", ex.Message);
}
catch (ProviderException ex)
{
    _logger.LogError("LLM provider error: {Message}", ex.Message);
}
catch (NetAgentException ex)
{
    _logger.LogError("General agent error: {Message}", ex.Message);
}
```

## Logging

NetAgent integrates with Microsoft.Extensions.Logging:

```csharp
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Information);
});
```

## Performance Optimization

### Memory Usage
- Configure appropriate memory limits
- Use Redis for large-scale deployments
- Implement memory cleanup strategies

```csharp
// Configure Redis with optimization settings
services.AddNetAgent(options => {
    options.UseMemory(MemoryType.Redis, config => {
        config.ConnectionString = "your-redis-connection";
        config.MaxItems = 10000;
        config.ExpiryDays = 30;
        config.EnableCompression = true;
    });
});
```

### LLM Optimization
- Use appropriate temperature settings
- Implement token optimization
- Configure request timeouts

### Caching
- Enable response caching
- Implement vector similarity caching
- Use distributed caching for scaling

## Monitoring and Metrics

NetAgent provides built-in monitoring capabilities:

```csharp
services.AddNetAgentMonitoring(options => {
    options.CollectMetrics = true;
    options.EnablePromptLogging = true;
    options.MetricsInterval = TimeSpan.FromMinutes(5);
});
```

Available metrics include:
- Request/Response times
- Token usage
- Memory utilization
- Tool execution statistics
- Error rates

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE.txt file for details.