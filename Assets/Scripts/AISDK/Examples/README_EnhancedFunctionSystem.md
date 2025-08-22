# Enhanced Agent Function Calling System

This enhanced system brings powerful **function calling capabilities** to the UniBuddhi AI SDK. Agents can now have **custom personalities** and use **Extensions as callable functions**, creating intelligent assistants that can perform real actions and access external data.

## 🎯 Core Concept

**Agents** = Custom Personality + Available Functions (Extensions)

The flow works like this:
```
User Input → Agent (with custom system prompt) → Extensions (as tools/functions) → AI Provider → Response
```

## 🚀 Quick Start

### 1. Basic Setup
```csharp
// Get the AI SDK
var aiSDK = AISDKCore.Instance;
aiSDK.SetApiKey(\"your-api-key\");

// Add function extensions
var calculator = gameObject.AddComponent<CalculatorExtension>();
aiSDK.AddFunctionExtension(calculator);

// Create a personality
var mathTutorPersonality = aiSDK.CreatePersonality(
    \"Math Tutor\",
    \"You are a helpful math tutor. Use your calculator functions to solve problems step-by-step.\",
    new List<string> { \"patient\", \"educational\", \"step-by-step\" },
    new List<string> { \"calculations\", \"problem_solving\" }
);

// Create enhanced agent
var agent = aiSDK.CreateEnhancedAgent(
    \"MathBot\",
    AgentType.Technical,
    mathTutorPersonality,
    new string[] { \"Calculator\" }
);

// Send message
aiSDK.SendMessageToEnhancedAgent(\"MathBot\", \"What's 25 * 47 + 133?\", false);
```

### 2. Available Function Extensions

#### CalculatorExtension
- **Functions**: `add`, `subtract`, `multiply`, `divide`, `power`, `sqrt`, `sin`, `cos`, `log`, `average`, `sum`, `min`, `max`
- **Use Case**: Mathematical calculations, statistics
- **Example**: \"Calculate the area of a circle with radius 5\"

#### WeatherInfoExtension  
- **Functions**: `get_current_weather`, `get_weather_forecast`, `get_weather_alerts`, `get_air_quality`, `get_historical_weather`
- **Use Case**: Weather information and forecasting
- **Example**: \"What's the weather like in Tokyo for the next 3 days?\"

#### KnowledgeSearchExtension
- **Functions**: `search_facts`, `get_definition`, `browse_category`, `get_random_fact`, `convert_units`
- **Use Case**: Information lookup, definitions, unit conversions
- **Example**: \"Tell me about photosynthesis and convert 5 feet to meters\"

## 🎭 Agent Personalities

Personalities define how agents behave and what they specialize in:

```csharp
// Helpful Assistant - General purpose
var assistant = aiSDK.CreatePersonality(
    \"Helpful Assistant\",
    \"You are a helpful AI assistant with access to various tools. \" +
    \"Use appropriate functions to provide accurate responses.\",
    new List<string> { \"helpful\", \"clear\", \"informative\" },
    new List<string> { \"calculations\", \"weather_info\", \"knowledge_search\" }
);

// Weather Expert - Specialized in weather
var weatherExpert = aiSDK.CreatePersonality(
    \"Weather Expert\",
    \"You are a weather expert with access to current weather data. \" +
    \"Provide detailed weather information and explain patterns.\",
    new List<string> { \"knowledgeable\", \"precise\", \"weather-focused\" },
    new List<string> { \"weather_info\", \"forecasting\", \"climate_analysis\" }
);

// Math Tutor - Educational focus
var mathTutor = aiSDK.CreatePersonality(
    \"Math Tutor\",
    \"You are a patient math tutor. Solve problems step-by-step \" +
    \"using calculator functions and explain your reasoning.\",
    new List<string> { \"patient\", \"educational\", \"step-by-step\" },
    new List<string> { \"calculations\", \"problem_solving\" }
);
```

## 🔧 Creating Custom Function Extensions

### 1. Basic Function Extension
```csharp
public class MyCustomExtension : BaseFunctionExtension
{
    public override string Name => \"MyCustom\";
    public override string Version => \"1.0.0\";
    public override string Description => \"My custom functions\";

    protected override void InitializeFunctions()
    {
        // Define a simple function
        var params = CreateParameters(new Dictionary<string, FunctionProperty>
        {
            [\"input\"] = CreateParameter(\"string\", \"Input text\", true)
        }, new List<string> { \"input\" });
        
        AddFunction(\"my_function\", \"Does something custom\", params, \"my_function('hello')\");
    }

    protected override IEnumerator ExecuteFunctionInternal(string functionName, Dictionary<string, object> arguments, Action<FunctionResult> onComplete)
    {
        if (functionName == \"my_function\")
        {
            var input = GetArgument<string>(arguments, \"input\");
            var result = $\"Processed: {input.ToUpper()}\";
            onComplete(new FunctionResult(functionName, Name, true, result));
        }
        else
        {
            onComplete(new FunctionResult(functionName, Name, false, \"\", \"Function not supported\"));
        }
        yield break;
    }

    protected override IEnumerator PerformTest(Action<bool> onComplete)
    {
        // Test your function
        onComplete(true);
        yield break;
    }
}
```

### 2. Advanced Function Extension with Multiple Functions
```csharp
public class FileManagerExtension : BaseFunctionExtension
{
    public override string Name => \"FileManager\";
    public override string Version => \"1.0.0\";
    public override string Description => \"File management functions\";

    protected override void InitializeFunctions()
    {
        // List files function
        var listParams = CreateParameters(new Dictionary<string, FunctionProperty>
        {
            [\"path\"] = CreateParameter(\"string\", \"Directory path\", false, null, \".\"),
            [\"pattern\"] = CreateParameter(\"string\", \"File pattern\", false, null, \"*.*\")
        });
        AddFunction(\"list_files\", \"List files in directory\", listParams);
        
        // Read file function
        var readParams = CreateParameters(new Dictionary<string, FunctionProperty>
        {
            [\"filename\"] = CreateParameter(\"string\", \"File to read\", true)
        }, new List<string> { \"filename\" });
        AddFunction(\"read_file\", \"Read file contents\", readParams);
    }

    protected override IEnumerator ExecuteFunctionInternal(string functionName, Dictionary<string, object> arguments, Action<FunctionResult> onComplete)
    {
        switch (functionName)
        {
            case \"list_files\":
                var path = GetArgument<string>(arguments, \"path\", \".\");
                var pattern = GetArgument<string>(arguments, \"pattern\", \"*.*\");
                // Implement file listing logic
                onComplete(new FunctionResult(functionName, Name, true, \"File list here\"));
                break;
                
            case \"read_file\":
                var filename = GetArgument<string>(arguments, \"filename\");
                // Implement file reading logic
                onComplete(new FunctionResult(functionName, Name, true, \"File contents here\"));
                break;
                
            default:
                onComplete(new FunctionResult(functionName, Name, false, \"\", \"Unknown function\"));
                break;
        }
        yield break;
    }

    protected override IEnumerator PerformTest(Action<bool> onComplete)
    {
        onComplete(true);
        yield break;
    }
}
```

## 🎮 Usage Examples

### Example Conversations

**Math Tutor Agent:**
```
User: \"What's the area of a circle with radius 7?\"
Agent: \"I'll calculate that for you step by step.

First, I need to use the formula: Area = π × r²

[Calls multiply function: 7 × 7 = 49]
[Calls multiply function: 49 × 3.14159 = 153.94]

The area of a circle with radius 7 is approximately 153.94 square units.\"
```

**Weather Expert Agent:**
```
User: \"Will it rain in London tomorrow?\"
Agent: \"Let me check the current weather and forecast for London.

[Calls get_weather_forecast function for London]

Based on the forecast, tomorrow in London will be cloudy with a 70% chance of rain in the afternoon. 
The temperature will range from 12°C to 18°C. I recommend bringing an umbrella!\"
```

**Research Assistant Agent:**
```
User: \"Tell me about gravity and calculate the force between two 10kg objects 1 meter apart\"
Agent: \"I'll explain gravity and calculate that for you.

[Calls search_facts function for \"gravity\"]

Gravity is a fundamental force that attracts objects with mass toward each other...

Now for the calculation using Newton's law of universal gravitation: F = G(m₁m₂)/r²

[Calls multiply function: 10 × 10 = 100]
[Calls multiply function: 6.674e-11 × 100 = 6.674e-9]
[Calls divide function: 6.674e-9 ÷ 1 = 6.674e-9]

The gravitational force between two 10kg objects 1 meter apart is 6.674 × 10⁻⁹ Newtons.\"
```

## 🔄 Architecture Overview

### Components

1. **EnhancedAgent**: Extended agent with function calling capabilities
2. **FunctionCallManager**: Manages function registration and execution
3. **BaseFunctionExtension**: Base class for creating function extensions
4. **AgentPersonality**: Defines agent behavior and specialization
5. **FunctionDefinition**: Describes available functions and parameters

### Flow Diagram
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   User Input    │───▶│ Enhanced Agent  │───▶│ Function Call   │
└─────────────────┘    │ + Personality   │    │   Manager       │
                       └─────────────────┘    └─────────────────┘
                               │                        │
                               ▼                        ▼
                       ┌─────────────────┐    ┌─────────────────┐
                       │   AI Provider   │    │ Function        │
                       │   (OpenAI/etc)  │    │ Extensions      │
                       └─────────────────┘    └─────────────────┘
                               │                        │
                               ▼                        ▼
                       ┌─────────────────────────────────────────┐
                       │           Final Response                │
                       └─────────────────────────────────────────┘
```

## 🎯 Best Practices

### 1. Agent Design
- **Specialized Personalities**: Create focused personalities for specific use cases
- **Clear Instructions**: Be explicit about when and how to use functions
- **Function Awareness**: Include function capabilities in system prompts

### 2. Function Extension Design
- **Single Responsibility**: Each extension should focus on one domain
- **Clear Descriptions**: Provide detailed function and parameter descriptions
- **Error Handling**: Always handle errors gracefully
- **Testing**: Implement comprehensive test methods

### 3. Function Definitions
- **Descriptive Names**: Use clear, self-explanatory function names
- **Detailed Parameters**: Describe each parameter's purpose and type
- **Examples**: Provide usage examples for complex functions

## 🚨 Important Notes

### Limitations
- Function calling depends on AI provider support
- Maximum function calls per message is configurable (default: 5)
- Function execution timeout is 30 seconds by default
- Some AI providers may not support function calling natively

### Performance
- Function calls are executed sequentially
- Large function results may impact response time
- Complex function chains may require multiple API calls

### Security
- Validate all function inputs
- Implement proper error handling
- Be cautious with file system operations
- Don't expose sensitive functionality

## 📚 Complete Integration Example

See [`EnhancedAgentFunctionExample.cs`](./EnhancedAgentFunctionExample.cs) for a complete UI integration example, or [`QuickStartFunctionExample.cs`](./QuickStartFunctionExample.cs) for a simple console-based example.

This enhanced system transforms static AI responses into dynamic, action-capable agents that can solve real problems using available tools!"