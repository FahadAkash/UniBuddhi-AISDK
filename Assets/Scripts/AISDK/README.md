# UniBuddhi AI SDK for Unity

A comprehensive, modular UniBuddhi AI SDK for Unity that supports multiple AI providers (OpenAI, Gemini, DeepSeek, Anthropic, Cohere) with unified interfaces, extension system, and TTS integration.

## Features

- **Multi-Provider Support**: OpenAI, Gemini, DeepSeek, Anthropic, Cohere
- **Modular Architecture**: Factory pattern, singleton pattern, interface-based design
- **Extension System**: Pluggable extensions for preprocessing and postprocessing
- **TTS Integration**: ElevenLabs, OpenAI, Azure, Google, Amazon TTS support
- **Agent Types**: Assistant, Creative, Technical, Analytical, Conversational
- **Streaming Support**: Real-time streaming responses
- **Unity Optimized**: Coroutine-based, non-blocking operations

## Quick Start

### 1. Setup

1. Add the `AISDKCore` component to a GameObject in your scene
2. Configure your API keys in the inspector
3. Select your preferred AI provider and TTS provider
4. Add extensions as needed

### 2. Basic Usage

```csharp
// Get the SDK instance
var sdk = AISDKCore.Instance;

// Send a message
sdk.SendMessage("Hello, what time is it?", AgentType.Assistant, true);

// Stream a message
sdk.StreamMessage("Tell me a story", AgentType.Creative, (chunk, isComplete) => {
    Debug.Log($"Chunk: {chunk}");
    if (isComplete) Debug.Log("Streaming complete!");
});
```

### 3. Event Handling

```csharp
// Subscribe to events
AISDKCore.OnAIResponse += (response) => Debug.Log($"AI: {response}");
AISDKCore.OnAudioGenerated += (clip) => Debug.Log($"Audio: {clip.name}");
AISDKCore.OnError += (error) => Debug.LogError($"Error: {error}");
```

## Architecture

### Core Components

```
AISDKCore (Singleton)
├── AIProviderFactory
│   ├── OpenAIProvider
│   ├── GeminiProvider
│   ├── DeepSeekProvider
│   └── ...
├── AgentFactory
│   ├── AssistantAgent
│   ├── CreativeAgent
│   ├── TechnicalAgent
│   └── ...
├── TTSProviderFactory
│   ├── ElevenLabsProvider
│   ├── OpenAITTSProvider
│   └── ...
└── Extension System
    ├── CurrentTimeExtension
    ├── WebSearchExtension
    └── ...
```

### Data Flow

```
User Input → Extensions (Preprocess) → AI Provider → AI Response → Extensions (Postprocess) → TTS → Audio
```

## AI Providers

### Supported Providers

- **OpenAI**: GPT-4, GPT-4 Turbo, GPT-3.5 Turbo
- **Gemini**: Gemini 1.5 Pro, Gemini 1.5 Flash, Gemini Pro
- **DeepSeek**: DeepSeek Coder, DeepSeek Chat
- **Anthropic**: Claude 3 Opus, Claude 3 Sonnet, Claude 3 Haiku
- **Cohere**: Command, Command Light

### Provider Configuration

```csharp
// Set API key
sdk.SetApiKey("your-api-key-here");

// Switch providers
sdk.SetProvider(AIProviderType.Gemini);
```

## Agent Types

### Available Agents

- **Assistant**: General helpful AI responses
- **Creative**: Imaginative and artistic responses
- **Technical**: Precise technical information
- **Analytical**: Data analysis and problem-solving
- **Conversational**: Friendly, engaging conversations

### Agent Configuration

```csharp
// Each agent has optimized settings
var config = new AgentConfig(
    AgentType.Creative,
    "You are a creative storyteller...",
    0.9f,  // Higher temperature for creativity
    1500   // More tokens for longer responses
);
```

## Extension System

### Creating Extensions

Extend `BaseExtension` to create custom extensions:

```csharp
public class MyCustomExtension : BaseExtension
{
    public override string Name => "MyExtension";
    public override string Version => "1.0.0";
    public override string Description => "My custom extension";

    protected override IEnumerator ProcessPreprocess(string userMessage)
    {
        // Add context before AI processing
        if (userMessage.Contains("weather"))
        {
            yield return "Current weather: Sunny, 25°C";
        }
        else
        {
            yield return "";
        }
    }

    protected override IEnumerator ProcessPostprocess(string modelText)
    {
        // Modify AI response after processing
        var modifiedText = modelText.Replace("AI", "Assistant");
        yield return new ExtensionResult(Name, modelText, modifiedText);
    }

    protected override IEnumerator PerformTest()
    {
        // Test your extension
        yield return true;
    }
}
```

### Built-in Extensions

- **CurrentTimeExtension**: Provides current time and date information
- **WebSearchExtension**: Adds web search context (requires SerpAPI)
- **LatestVideoExtension**: YouTube video information (requires YouTube API)

### Extension Configuration

```csharp
// Add extension
var extension = gameObject.AddComponent<CurrentTimeExtension>();
sdk.AddExtension(extension);

// Remove extension
sdk.RemoveExtension(extension);
```

## TTS Integration

### Supported TTS Providers

- **ElevenLabs**: High-quality voice synthesis
- **OpenAI**: TTS API
- **Azure**: Speech Services
- **Google**: Text-to-Speech
- **Amazon**: Polly

### TTS Usage

```csharp
// Generate speech
sdk.SendMessage("Hello world", AgentType.Assistant, true); // true = generate speech

// Control audio
sdk.SetVolume(0.8f);
sdk.StopAudio();
```

## Advanced Usage

### Custom Provider Implementation

```csharp
public class CustomProvider : IAIProvider
{
    public AIProviderType Type => AIProviderType.Custom;
    public string Name => "CustomProvider";
    public bool IsInitialized { get; private set; }

    public void Initialize(ProviderConfig config)
    {
        // Initialize your provider
        IsInitialized = true;
    }

    public IEnumerator ChatAsync(ChatRequest request, Action<AIResponse> onComplete)
    {
        // Implement your chat logic
        var response = new AIResponse(true, "Custom response");
        onComplete?.Invoke(response);
        yield break;
    }

    // Implement other interface methods...
}
```

### Custom Agent Implementation

```csharp
public class CustomAgent : BaseAgent
{
    public CustomAgent() : base(AgentType.Custom)
    {
    }

    protected override ChatRequest CreateChatRequest(Message[] messages)
    {
        var request = base.CreateChatRequest(messages);
        // Add custom logic
        return request;
    }
}
```

### Factory Registration

```csharp
// Register custom provider
AIProviderFactory.RegisterProvider(AIProviderType.Custom, typeof(CustomProvider));

// Register custom agent
AgentFactory.RegisterAgent(AgentType.Custom, typeof(CustomAgent));
```

## Configuration

### Inspector Settings

- **AI Provider**: Select your preferred AI provider
- **API Key**: Your provider API key
- **Base URL**: Custom API endpoint (optional)
- **TTS Provider**: Select TTS service
- **TTS API Key**: TTS service API key
- **Voice ID**: TTS voice identifier
- **Extensions**: Enable/disable extension system
- **Debug Logs**: Enable detailed logging

### Runtime Configuration

```csharp
// Configure at runtime
sdk.SetApiKey("new-api-key");
sdk.SetProvider(AIProviderType.OpenAI);
sdk.SetVolume(0.5f);
```

## Error Handling

The SDK provides comprehensive error handling:

```csharp
AISDKCore.OnError += (error) => {
    Debug.LogError($"AI SDK Error: {error}");
    // Handle error appropriately
};
```

## Performance

- **Non-blocking**: All operations use Unity coroutines
- **Memory Efficient**: Proper disposal of resources
- **Caching**: Intelligent caching of responses
- **Statistics**: Built-in performance monitoring

## Examples

See `AISDKExample.cs` for complete usage examples including:

- Basic chat functionality
- Streaming responses
- Extension usage
- TTS integration
- UI integration
- Error handling

## Troubleshooting

### Common Issues

1. **"AI SDK not initialized"**
   - Ensure AISDKCore component is in the scene
   - Check API keys are configured

2. **"Provider not available"**
   - Verify provider is registered
   - Check API key is valid

3. **"Extension not working"**
   - Ensure extension inherits from BaseExtension
   - Check extension is enabled
   - Verify ShouldRespond() logic

### Debug Mode

Enable debug logs in the inspector to see detailed information about:
- Provider initialization
- API requests/responses
- Extension processing
- TTS generation
- Error details

## Contributing

To add new providers, agents, or extensions:

1. Implement the appropriate interface
2. Register with the factory
3. Add configuration options
4. Update documentation
5. Add tests

## License

This SDK is provided as-is for educational and development purposes.

## Support

For issues and questions:
1. Check the troubleshooting section
2. Review example code
3. Enable debug logs
4. Check API documentation for your provider
