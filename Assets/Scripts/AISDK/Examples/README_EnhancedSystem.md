# UniBuddhi AI SDK - Enhanced System Guide

## Overview

The enhanced UniBuddhi AI SDK provides a powerful, flexible system for creating AI agents with custom personalities, extensions, and real-time capabilities. This system transforms the SDK from a simple AI provider into a comprehensive AI brain that can manage multiple agents, each with their own unique characteristics and abilities.

## Key Features

### 🧠 **AISDK Core - The Main Brain**
- **Central Management**: Manages all agents, extensions, and AI providers
- **Event System**: Comprehensive event broadcasting for system-wide communication
- **Extension Registry**: Centralized extension management with capability tracking
- **Agent Lifecycle**: Full agent creation, configuration, and management

### 🤖 **Enhanced Agents with Personalities**
- **Custom Personalities**: Each agent can have unique traits, system prompts, and behaviors
- **Dynamic Configuration**: Runtime personality and extension management
- **Action Capabilities**: Agents can perform actions based on their extensions
- **Context Awareness**: Agents understand their capabilities and limitations

### 🔧 **Extension System**
- **Workable Extensions**: Real-time capabilities like weather, TTS, web search
- **Capability Registry**: Extensions declare what they can do
- **Agent Binding**: Extensions can be dynamically added/removed from agents
- **Performance Tracking**: Monitor extension performance and health

### 📡 **Event System**
- **Real-time Communication**: Events for agent actions, extension operations, and system changes
- **Decoupled Architecture**: Components communicate through events
- **Debugging Support**: Comprehensive event logging and monitoring
- **Custom Events**: Create custom event types for your specific needs

## Quick Start

### 1. Basic Setup

```csharp
// Get the SDK instance
var sdk = AISDKCore.Instance;

// Wait for initialization
while (!sdk.IsInitialized)
{
    yield return new WaitForSeconds(0.1f);
}
```

### 2. Create an Agent with Personality

```csharp
// Create a farmer personality
var farmerPersonality = new AgentPersonality(
    "Farmer",
    "A wise and experienced farmer who knows about crops and weather",
    "You are a wise farmer with deep knowledge of agriculture...",
    0.8f // Higher temperature for creative responses
);

// Add personality traits
farmerPersonality.Traits = new string[] { "Wise", "Experienced", "Nature-loving" };
farmerPersonality.Creativity = 0.7f;
farmerPersonality.Formality = 0.3f;

// Create agent with extensions
var extensions = new string[] { "WeatherExtension", "TTSExtension" };
var farmerAgent = sdk.CreateAgent("FarmerAgent", AgentType.Creative, farmerPersonality, extensions);
```

### 3. Subscribe to Events

```csharp
// Subscribe to agent events
AISDKCore.OnAgentEvent += (agentEvent) => {
    Debug.Log($"Agent {agentEvent.AgentId} performed {agentEvent.Action}");
};

// Subscribe to extension events
AISDKCore.OnExtensionEvent += (extensionEvent) => {
    if (extensionEvent.Success)
        Debug.Log($"Extension {extensionEvent.ExtensionName} completed {extensionEvent.Operation}");
    else
        Debug.LogError($"Extension failed: {extensionEvent.ErrorMessage}");
};

// Subscribe to system events
AISDKCore.OnSystemMessage += (message) => {
    Debug.Log($"[SYSTEM] {message}");
};
```

### 4. Manage Extensions

```csharp
// Add extension to agent
sdk.AddExtensionToAgent("FarmerAgent", "WeatherExtension");

// Remove extension from agent
sdk.RemoveExtensionFromAgent("FarmerAgent", "WeatherExtension");

// Check agent capabilities
var extensions = sdk.GetAgentExtensions("FarmerAgent");
var capabilities = sdk.GetExtensionCapabilities("WeatherExtension");
```

## Agent Personalities

### Personality Structure

```csharp
public class AgentPersonality
{
    public string Name { get; set; }                    // "Farmer", "Teacher", "Scientist"
    public string Description { get; set; }             // Human-readable description
    public string SystemPrompt { get; set; }            // AI system prompt
    public float Temperature { get; set; }              // 0.0 (focused) to 1.0 (creative)
    public float Creativity { get; set; }               // 0.0 (logical) to 1.0 (imaginative)
    public float Formality { get; set; }                // 0.0 (casual) to 1.0 (formal)
    public string[] Traits { get; set; }                // ["Wise", "Patient", "Creative"]
    public Dictionary<string, object> CustomSettings { get; set; }
}
```

### Example Personalities

#### Farmer Agent
```csharp
var farmerPersonality = new AgentPersonality(
    "Farmer",
    "A wise and experienced farmer",
    "You are a wise farmer with deep knowledge of agriculture, crop management, " +
    "weather patterns, and sustainable farming practices. You speak in a friendly, " +
    "down-to-earth manner and love sharing farming wisdom.",
    0.8f
);
farmerPersonality.Traits = new string[] { "Wise", "Experienced", "Nature-loving", "Practical" };
farmerPersonality.Creativity = 0.7f;
farmerPersonality.Formality = 0.3f;
```

#### Teacher Agent
```csharp
var teacherPersonality = new AgentPersonality(
    "Teacher",
    "A knowledgeable and patient teacher",
    "You are a knowledgeable teacher who excels at breaking down complex topics " +
    "into understandable concepts. You use examples, analogies, and clear explanations.",
    0.6f
);
teacherPersonality.Traits = new string[] { "Knowledgeable", "Patient", "Encouraging", "Clear" };
teacherPersonality.Creativity = 0.5f;
teacherPersonality.Formality = 0.6f;
```

#### Scientist Agent
```csharp
var scientistPersonality = new AgentPersonality(
    "Scientist",
    "A precise and analytical researcher",
    "You are a precise scientist focused on accuracy and evidence. You provide " +
    "detailed explanations, cite sources when possible, and maintain scientific rigor.",
    0.4f
);
scientistPersonality.Traits = new string[] { "Precise", "Analytical", "Evidence-based", "Thorough" };
scientistPersonality.Creativity = 0.3f;
scientistPersonality.Formality = 0.8f;
```

## Extension System

### Creating Extensions

Extensions inherit from `BaseExtension` and provide specific capabilities:

```csharp
public class WeatherExtension : BaseExtension
{
    public override string Name => "WeatherExtension";
    public override string Version => "1.0.0";
    public override string Description => "Provides real-time weather information";
    
    protected override IEnumerator ProcessPreprocess(string userMessage)
    {
        if (ShouldRespond(userMessage))
        {
            var weatherInfo = GetWeatherInformation();
            yield return new ExtensionContext(Name, weatherInfo, 1);
        }
        else
        {
            yield return new ExtensionContext(Name, "", 0);
        }
    }
    
    protected override bool ShouldRespond(string userMessage)
    {
        var lowerMessage = userMessage.ToLower();
        return lowerMessage.Contains("weather") || lowerMessage.Contains("temperature");
    }
    
    // Extension-specific methods
    public string GetWeatherInformation() { /* ... */ }
    public string GetWeatherForecast(int days) { /* ... */ }
}
```

### Extension Capabilities

Extensions declare their capabilities for the system to understand:

```csharp
public ExtensionCapability[] GetCapabilities()
{
    return new ExtensionCapability[]
    {
        new ExtensionCapability("Weather Information", "Provides real-time weather data")
        {
            SupportedActions = new string[] { "GetCurrentWeather", "GetForecast", "SetCity" },
            IsActive = true,
            PerformanceScore = 0.9f
        }
    };
}
```

### Built-in Extensions

- **CurrentTimeExtension**: Provides current time and date
- **WeatherExtension**: Real-time weather information and forecasts
- **PersonalityAgentExtension**: Enhanced personality management
- **TTS Extension**: Text-to-speech capabilities

## Event System

### Event Types

#### SDK Events
```csharp
// Subscribe to all SDK events
AISDKCore.OnSDKEvent += (sdkEvent) => {
    Debug.Log($"Event: {sdkEvent.EventType} from {sdkEvent.Source}");
};
```

#### Agent Events
```csharp
// Subscribe to agent-specific events
AISDKCore.OnAgentEvent += (agentEvent) => {
    Debug.Log($"Agent {agentEvent.AgentId}: {agentEvent.Action}");
    Debug.Log($"Result: {agentEvent.Result}");
};
```

#### Extension Events
```csharp
// Subscribe to extension events
AISDKCore.OnExtensionEvent += (extensionEvent) => {
    var status = extensionEvent.Success ? "✅" : "❌";
    Debug.Log($"{status} {extensionEvent.ExtensionName}: {extensionEvent.Operation}");
};
```

#### System Events
```csharp
// Subscribe to system messages
AISDKCore.OnSystemMessage += (message) => {
    Debug.Log($"[SYSTEM] {message}");
};
```

### Broadcasting Events

```csharp
// Send system message
sdk.SendSystemMessage("Weather extension updated");

// Broadcast custom event
var customEvent = new AISDKEvent("CustomAction", "MyComponent", "Custom data");
sdk.BroadcastEvent(customEvent);
```

## Advanced Usage

### Dynamic Agent Management

```csharp
// Create multiple agents
var farmerAgent = sdk.CreateAgent("Farmer", AgentType.Creative, farmerPersonality);
var teacherAgent = sdk.CreateAgent("Teacher", AgentType.Assistant, teacherPersonality);

// Switch between agents
var currentAgent = sdk.AllAgents["Farmer"];

// Update agent personality
var newPersonality = new AgentPersonality("Expert Farmer", "...");
sdk.SetAgentPersonality("Farmer", newPersonality);
```

### Extension Management

```csharp
// Register extension with capabilities
var weatherExt = new WeatherExtension();
sdk.RegisterExtension(weatherExt, weatherExt.GetCapabilities());

// Check if agent can perform action
if (sdk.CanAgentPerformAction("Farmer", "GetCurrentWeather"))
{
    Debug.Log("Farmer can check weather!");
}

// Get all extension capabilities
foreach (var extName in sdk.GetAgentExtensions("Farmer"))
{
    var capabilities = sdk.GetExtensionCapabilities(extName);
    foreach (var cap in capabilities)
    {
        Debug.Log($"Capability: {cap.Name} - {cap.Description}");
    }
}
```

### Performance Monitoring

```csharp
// Get agent statistics
var agentStats = farmerAgent.GetStatistics();
Debug.Log($"Messages: {agentStats["total_messages"]}");

// Get extension performance
var extCapabilities = sdk.GetExtensionCapabilities("WeatherExtension");
foreach (var cap in extCapabilities)
{
    Debug.Log($"Performance: {cap.PerformanceScore}");
}
```

## Best Practices

### 1. Agent Design
- **Clear Personalities**: Define distinct, memorable personalities
- **Appropriate Temperature**: Use lower temperatures for precise tasks, higher for creative ones
- **Consistent Traits**: Maintain personality consistency across interactions

### 2. Extension Development
- **Single Responsibility**: Each extension should do one thing well
- **Capability Declaration**: Always declare what your extension can do
- **Error Handling**: Gracefully handle failures and report them
- **Performance**: Cache data when possible and optimize for speed

### 3. Event Usage
- **Subscribe Early**: Subscribe to events in Start() or Awake()
- **Unsubscribe**: Always unsubscribe in OnDestroy()
- **Event Filtering**: Filter events by type or source when needed
- **Async Operations**: Use coroutines for long-running operations

### 4. Memory Management
- **Agent Cleanup**: Remove agents when no longer needed
- **Extension Cleanup**: Unregister extensions properly
- **Event Cleanup**: Unsubscribe from events to prevent memory leaks

## Troubleshooting

### Common Issues

1. **Agent Not Responding**
   - Check if agent is initialized
   - Verify agent has required extensions
   - Check agent personality configuration

2. **Extensions Not Working**
   - Ensure extension is registered
   - Check extension capabilities
   - Verify extension is bound to agent

3. **Events Not Firing**
   - Check event subscription timing
   - Verify event names match
   - Check for null references

### Debug Mode

Enable debug logging in the inspector:
```csharp
// In AISDKCore inspector
[SerializeField] private bool showDebugLogs = true;
[SerializeField] private bool enablePerformanceMonitoring = true;
```

## Example Scenarios

### Scenario 1: Smart Home Assistant
```csharp
// Create home assistant with multiple capabilities
var homeAssistant = sdk.CreateAgent("HomeAssistant", AgentType.Assistant, homePersonality);
sdk.AddExtensionToAgent("HomeAssistant", "WeatherExtension");
sdk.AddExtensionToAgent("HomeAssistant", "TTSExtension");
sdk.AddExtensionToAgent("HomeAssistant", "SmartHomeExtension");

// Ask about weather
homeAssistant.ChatAsync("What's the weather like today?");
```

### Scenario 2: Educational Game
```csharp
// Create teacher agent for educational content
var teacher = sdk.CreateAgent("GameTeacher", AgentType.Assistant, teacherPersonality);
sdk.AddExtensionToAgent("GameTeacher", "MathExtension");
sdk.AddExtensionToAgent("GameTeacher", "ScienceExtension");

// Ask for math help
teacher.ChatAsync("Can you help me solve 2x + 5 = 13?");
```

### Scenario 3: Customer Service Bot
```csharp
// Create customer service agent
var customerService = sdk.CreateAgent("CustomerService", AgentType.Assistant, servicePersonality);
sdk.AddExtensionToAgent("CustomerService", "KnowledgeBaseExtension");
sdk.AddExtensionToAgent("CustomerService", "TTSExtension");

// Handle customer inquiry
customerService.ChatAsync("I need help with my order");
```

## Conclusion

The enhanced UniBuddhi AI SDK provides a powerful foundation for building sophisticated AI applications. With its flexible agent system, comprehensive extension framework, and real-time event system, you can create AI experiences that are both intelligent and engaging.

Key benefits:
- **Modular Design**: Easy to add new capabilities and agents
- **Real-time Communication**: Events keep everything synchronized
- **Flexible Personalities**: Create unique, engaging AI characters
- **Extensible Architecture**: Build custom extensions for your needs
- **Professional Quality**: Production-ready with comprehensive error handling

Start building your AI applications today with the enhanced UniBuddhi AI SDK!
