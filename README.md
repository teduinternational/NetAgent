# NetAgent

A modular .NET framework for building AI agents with multiple LLM providers.

## Features

- Multiple LLM provider support:
  - OpenAI
  - Azure OpenAI
  - Ollama
- Built-in memory providers
- Extensible planning system
- Standard tool collection
- Exception handling and logging
- Configuration validation

## Installation

```bash
dotnet add package NetAgent
```

## Quick Start

1. Add configuration to appsettings.json:

```json
{
  "NetAgent": {
    "Provider": "openai",
    "OpenAI": {
      "ApiKey": "your-api-key",
      "Model": "gpt-4"
    },
    "Tools": ["calculator", "websearch"]
  }
}
```

2. Register services:

```csharp
services.AddNetAgentFromConfig(configuration);
```

3. Use the agent:

```csharp
public class MyService 
{
    private readonly IAgent _agent;

    public MyService(IAgent agent)
    {
        _agent = agent;
    }

    public async Task<string> GetResponse(string input)
    {
        var response = await _agent.ProcessAsync(new AgentRequest 
        {
            Input = input
        });
        return response.Output;
    }
}
```

## Configuration

### OpenAI Provider
```json
{
  "NetAgent": {
    "Provider": "openai",
    "OpenAI": {
      "ApiKey": "your-api-key",
      "Model": "gpt-4"
    }
  }
}
```

### Azure OpenAI Provider
```json
{
  "NetAgent": {
    "Provider": "azureopenai",
    "AzureOpenAI": {
      "Endpoint": "your-endpoint",
      "ApiKey": "your-api-key",
      "DeploymentName": "your-deployment"
    }
  }
}
```

### Ollama Provider
```json
{
  "NetAgent": {
    "Provider": "ollama",
    "Ollama": {
      "Endpoint": "http://localhost:11434",
      "Model": "llama2"
    }
  }
}
```

## Available Tools

- Calculator: Performs mathematical calculations
- WebSearch: Searches the web for information

## Memory Providers

- InMemory: Stores conversation history in memory
- Redis: Stores conversation history in Redis (requires additional package)

## Extension Points

### Custom Tools

```csharp
public class MyCustomTool : IAgentTool
{
    public string Name => "mycustom";
    
    public async Task<string> ExecuteAsync(string input)
    {
        // Implement your tool logic here
        return result;
    }
}

// Register:
services.AddSingleton<IAgentTool, MyCustomTool>();
```

### Custom Memory Provider

```csharp
public class MyMemoryStore : IMemoryStore
{
    public async Task SaveAsync(Memory memory)
    {
        // Implement save logic
    }

    public async Task<IEnumerable<Memory>> LoadAsync(int limit = 10)
    {
        // Implement load logic
        return memories;
    }
}

// Register:
services.AddSingleton<IMemoryStore, MyMemoryStore>();
```

## Error Handling

The framework uses custom exceptions:
- `NetAgentException`: Base exception
- `ConfigurationException`: Configuration related errors
- `ProviderException`: Provider related errors
- `ToolException`: Tool execution errors

## Logging

The framework integrates with Microsoft.Extensions.Logging:

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Submit a pull request

## License

MIT