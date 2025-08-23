using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UniBuddhi.Core.Models
{
    #region Enums
    /// <summary>
    /// Supported AI providers
    /// </summary>
    public enum AIProviderType
    {
        OpenAI,
        Gemini,
        DeepSeek,
        Anthropic,
        Cohere,
        Custom
    }

    /// <summary>
    /// Supported TTS providers
    /// </summary>
    public enum TTSProviderType
    {
        ElevenLabs,
        OpenAI,
        Azure,
        Google,
        Amazon,
        Custom
    }

    /// <summary>
    /// Agent personality types
    /// </summary>
    public enum AgentType
    {
        Assistant,
        Creative,
        Technical,
        Analytical,
        Conversational,
        Custom
    }

    /// <summary>
    /// Model types for different providers
    /// </summary>
    public enum ModelType
    {
        // OpenAI Models
        GPT_4,
        GPT_4_Turbo,
        GPT_3_5_Turbo,
        
        // Gemini Models
        Gemini_1_5_Pro,
        Gemini_1_5_Flash,
        Gemini_Pro,
        
        // DeepSeek Models
        DeepSeek_Coder,
        DeepSeek_Chat,
        
        // Anthropic Models
        Claude_3_Opus,
        Claude_3_Sonnet,
        Claude_3_Haiku,
        
        // Custom
        Custom
    }
    #endregion

    #region Core Models
    /// <summary>
    /// AI response structure
    /// </summary>
    [Serializable]
    public class AIResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; }
        public string Error { get; set; }
        public AgentType AgentType { get; set; }
        public AIProviderType ProviderType { get; set; }
        public ModelType ModelType { get; set; }
        public long TokensUsed { get; set; }
        public float ResponseTime { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public AIResponse(bool success, string content, string error = "", AgentType agentType = AgentType.Assistant)
        {
            Success = success;
            Content = content;
            Error = error;
            AgentType = agentType;
            Metadata = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Message structure for conversations
    /// </summary>
    [Serializable]
    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public Message(string role, string content)
        {
            Role = role;
            Content = content;
            Timestamp = DateTime.Now;
            Metadata = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Agent configuration
    /// </summary>
    [Serializable]
    public class AgentConfig
    {
        public AgentType Type { get; set; }
        public string SystemPrompt { get; set; }
        public float Temperature { get; set; }
        public int MaxTokens { get; set; }
        public ModelType Model { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public AgentConfig(AgentType type, string systemPrompt = "", float temperature = 0.7f, int maxTokens = 1000, ModelType model = ModelType.GPT_3_5_Turbo)
        {
            Type = type;
            SystemPrompt = systemPrompt;
            Temperature = temperature;
            MaxTokens = maxTokens;
            Model = model;
            Description = string.Empty;
            Parameters = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Provider configuration
    /// </summary>
    [Serializable]
    public class ProviderConfig
    {
        public AIProviderType Type { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public string Organization { get; set; }
        public ModelType Model { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public ProviderConfig(AIProviderType type, string apiKey = "", string baseUrl = "", ModelType model = ModelType.GPT_3_5_Turbo)
        {
            Type = type;
            ApiKey = apiKey;
            BaseUrl = baseUrl;
            Model = model;
            Organization = string.Empty;
            Parameters = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// TTS configuration
    /// </summary>
    [Serializable]
    public class TTSConfig
    {
        public TTSProviderType Type { get; set; }
        public string ApiKey { get; set; }
        public string VoiceId { get; set; }
        public string BaseUrl { get; set; }
        public float Speed { get; set; }
        public float Pitch { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public TTSConfig(TTSProviderType type, string apiKey = "", string voiceId = "")
        {
            Type = type;
            ApiKey = apiKey;
            VoiceId = voiceId;
            BaseUrl = string.Empty;
            Speed = 1.0f;
            Pitch = 1.0f;
            Parameters = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Extension configuration
    /// </summary>
    [Serializable]
    public class ExtensionConfig
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool Enabled { get; set; }
        public int Priority { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public ExtensionConfig(string name, string version = "1.0.0", bool enabled = true, int priority = 0)
        {
            Name = name;
            Version = version;
            Enabled = enabled;
            Priority = priority;
            Parameters = new Dictionary<string, object>();
        }
    }
    #endregion

    #region Request/Response Models
    /// <summary>
    /// Chat request structure
    /// </summary>
    [Serializable]
    public class ChatRequest
    {
        [JsonProperty("messages")]
        public List<Message> Messages { get; set; }
        
        [JsonProperty("model")]
        public string Model { get; set; }
        
        [JsonProperty("model_type")]
        public ModelType ModelType { get; set; }
        
        [JsonProperty("temperature")]
        public float Temperature { get; set; }
        
        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }
        
        [JsonProperty("stream")]
        public bool Stream { get; set; }
        
        [JsonProperty("system_prompt")]
        public string SystemPrompt { get; set; }

        public ChatRequest()
        {
            Messages = new List<Message>();
            Temperature = 0.7f;
            MaxTokens = 1000;
            Stream = false;
        }
    }

    /// <summary>
    /// Chat response structure
    /// </summary>
    [Serializable]
    public class ChatResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; }
        
        [JsonProperty("usage")]
        public Usage Usage { get; set; }
        
        [JsonProperty("model")]
        public string Model { get; set; }

        public ChatResponse()
        {
            Choices = new List<Choice>();
        }
    }

    [Serializable]
    public class Choice
    {
        [JsonProperty("index")]
        public int Index { get; set; }
        
        [JsonProperty("message")]
        public Message Message { get; set; }
        
        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
    }

    [Serializable]
    public class Usage
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }
        
        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }
        
        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// Streaming response structure
    /// </summary>
    [Serializable]
    public class StreamingResponse
    {
        public bool IsComplete { get; set; }
        public string Content { get; set; }
        public string FinishReason { get; set; }
        public AgentType AgentType { get; set; }
        public AIProviderType ProviderType { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public StreamingResponse(bool isComplete, string content, string finishReason = "", AgentType agentType = AgentType.Assistant)
        {
            IsComplete = isComplete;
            Content = content;
            FinishReason = finishReason;
            AgentType = agentType;
            Metadata = new Dictionary<string, object>();
        }
    }
    #endregion

    #region Extension Models
    /// <summary>
    /// Extension context for preprocessing
    /// </summary>
    [Serializable]
    public class ExtensionContext
    {
        public string ExtensionName { get; set; }
        public string Context { get; set; }
        public int Priority { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public DateTime Timestamp { get; set; }

        public ExtensionContext(string extensionName, string context, int priority = 0)
        {
            ExtensionName = extensionName;
            Context = context;
            Priority = priority;
            Metadata = new Dictionary<string, object>();
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// Extension result for postprocessing
    /// </summary>
    [Serializable]
    public class ExtensionResult
    {
        public string ExtensionName { get; set; }
        public string OriginalText { get; set; }
        public string ModifiedText { get; set; }
        public bool WasModified { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public DateTime Timestamp { get; set; }

        public ExtensionResult(string extensionName, string originalText, string modifiedText = null)
        {
            ExtensionName = extensionName;
            OriginalText = originalText;
            ModifiedText = modifiedText ?? originalText;
            WasModified = originalText != ModifiedText;
            Metadata = new Dictionary<string, object>();
            Timestamp = DateTime.Now;
        }
    }
    #endregion

    #region Function Calling Models
    /// <summary>
    /// Function definition for AI function calling
    /// </summary>
    [Serializable]
    public class FunctionDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("parameters")]
        public FunctionParameters Parameters { get; set; }
        
        public string ExtensionName { get; set; }
        public int Priority { get; set; }

        public FunctionDefinition(string name, string description, FunctionParameters parameters, string extensionName = "", int priority = 0)
        {
            Name = name;
            Description = description;
            Parameters = parameters;
            ExtensionName = extensionName;
            Priority = priority;
        }
    }

    /// <summary>
    /// Function parameters schema
    /// </summary>
    [Serializable]
    public class FunctionParameters
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "object";
        
        [JsonProperty("properties")]
        public Dictionary<string, FunctionProperty> Properties { get; set; }
        
        [JsonProperty("required")]
        public List<string> Required { get; set; }

        public FunctionParameters()
        {
            Properties = new Dictionary<string, FunctionProperty>();
            Required = new List<string>();
        }
    }

    /// <summary>
    /// Function property definition
    /// </summary>
    [Serializable]
    public class FunctionProperty
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("enum")]
        public List<string> Enum { get; set; }
        
        [JsonProperty("default")]
        public object Default { get; set; }

        public FunctionProperty(string type, string description, List<string> enumValues = null, object defaultValue = null)
        {
            Type = type;
            Description = description;
            Enum = enumValues;
            Default = defaultValue;
        }
    }

    /// <summary>
    /// Function call request
    /// </summary>
    [Serializable]
    public class FunctionCall
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("arguments")]
        public string Arguments { get; set; }
        
        public string ExtensionName { get; set; }
        public Dictionary<string, object> ParsedArguments { get; set; }

        public FunctionCall(string name, string arguments, string extensionName = "")
        {
            Name = name;
            Arguments = arguments;
            ExtensionName = extensionName;
            ParsedArguments = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Function call result
    /// </summary>
    [Serializable]
    public class FunctionResult
    {
        public string FunctionName { get; set; }
        public string ExtensionName { get; set; }
        public bool Success { get; set; }
        public string Result { get; set; }
        public string Error { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public DateTime Timestamp { get; set; }

        public FunctionResult(string functionName, string extensionName, bool success, string result, string error = "")
        {
            FunctionName = functionName;
            ExtensionName = extensionName;
            Success = success;
            Result = result;
            Error = error;
            Metadata = new Dictionary<string, object>();
            Timestamp = DateTime.Now;
        }
    }
    #endregion

    #region Enhanced Agent Models
    /// <summary>
    /// Enhanced agent configuration with function calling support
    /// </summary>
    [Serializable]
    public class EnhancedAgentConfig : AgentConfig
    {
        public List<string> ExtensionNames { get; set; }
        public List<FunctionDefinition> AvailableFunctions { get; set; }
        public bool EnableFunctionCalling { get; set; }
        public AgentPersonality Personality { get; set; }
        public Dictionary<string, object> FunctionSettings { get; set; }
        public string[] AllowedActions { get; set; }

        public EnhancedAgentConfig(AgentType type, AgentPersonality personality = null, string systemPrompt = "", float temperature = 0.7f, int maxTokens = 1000, ModelType model = ModelType.GPT_3_5_Turbo) 
            : base(type, systemPrompt, temperature, maxTokens, model)
        {
            ExtensionNames = new List<string>();
            AvailableFunctions = new List<FunctionDefinition>();
            EnableFunctionCalling = true;
            Personality = personality;
            FunctionSettings = new Dictionary<string, object>();
            AllowedActions = new string[0];
        }
    }

    /// <summary>
    /// Agent personality definition
    /// </summary>
    [Serializable]
    public class AgentPersonality
    {
        public string Name { get; set; }
        public string SystemPrompt { get; set; }
        public string Description { get; set; }
        public Dictionary<string, float> Traits { get; set; }
        public List<string> Capabilities { get; set; }
        public Dictionary<string, string> ResponsePatterns { get; set; }
        public Dictionary<string, object> Settings { get; set; }
        public float Temperature { get; set; }
        public int MaxTokens { get; set; }
        public float Creativity { get; set; }
        public float Formality { get; set; }

        public AgentPersonality(string name, string systemPrompt = "", string description = "")
        {
            Name = name;
            SystemPrompt = systemPrompt;
            Description = description;
            Temperature = 0.7f;
            MaxTokens = 1000;
            Creativity = 0.5f;
            Formality = 0.5f;
            Traits = new Dictionary<string, float>();
            Capabilities = new List<string>();
            ResponsePatterns = new Dictionary<string, string>();
            Settings = new Dictionary<string, object>();
        }

        public AgentPersonality(string name, string description, string systemPrompt, float temperature)
        {
            Name = name;
            Description = description;
            SystemPrompt = systemPrompt;
            Temperature = temperature;
            MaxTokens = 1000;
            Creativity = 0.5f;
            Formality = 0.5f;
            Traits = new Dictionary<string, float>();
            Capabilities = new List<string>();
            ResponsePatterns = new Dictionary<string, string>();
            Settings = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Enhanced chat request with function calling support
    /// </summary>
    [Serializable]
    public class EnhancedChatRequest : ChatRequest
    {
        [JsonProperty("functions")]
        public List<FunctionDefinition> Functions { get; set; }
        
        [JsonProperty("function_call")]
        public object FunctionCall { get; set; }
        
        public bool SupportsFunctionCalling { get; set; }
        public List<string> EnabledExtensions { get; set; }

        public EnhancedChatRequest() : base()
        {
            Functions = new List<FunctionDefinition>();
            SupportsFunctionCalling = true;
            EnabledExtensions = new List<string>();
        }
    }
    #endregion

    #region TTS Models
    /// <summary>
    /// TTS request structure
    /// </summary>
    [Serializable]
    public class TTSRequest
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("voice_id")]
        public string VoiceId { get; set; }
        
        [JsonProperty("model_id")]
        public string ModelId { get; set; }
        
        [JsonProperty("voice_settings")]
        public VoiceSettings VoiceSettings { get; set; }

        public TTSRequest(string text, string voiceId = "", string modelId = "eleven_monolingual_v1")
        {
            Text = text;
            VoiceId = voiceId;
            ModelId = modelId;
            VoiceSettings = new VoiceSettings();
        }
    }

    [Serializable]
    public class VoiceSettings
    {
        [JsonProperty("stability")]
        public float Stability { get; set; }
        
        [JsonProperty("similarity_boost")]
        public float SimilarityBoost { get; set; }
        
        [JsonProperty("style")]
        public float Style { get; set; }
        
        [JsonProperty("use_speaker_boost")]
        public bool UseSpeakerBoost { get; set; }

        public VoiceSettings()
        {
            Stability = 0.5f;
            SimilarityBoost = 0.5f;
            Style = 0.5f;
            UseSpeakerBoost = true;
        }
    }
    #endregion

    #region Agent Extension Models
    /// <summary>
    /// Agent extension binding - what extensions an agent can use
    /// </summary>
    [Serializable]
    public class AgentExtensionBinding
    {
        public string AgentId { get; set; }
        public string ExtensionName { get; set; }
        public bool IsEnabled { get; set; }
        public int Priority { get; set; }
        public Dictionary<string, object> ExtensionSettings { get; set; }
        public string[] AllowedActions { get; set; }

        public AgentExtensionBinding(string agentId, string extensionName)
        {
            AgentId = agentId;
            ExtensionName = extensionName;
            IsEnabled = true;
            Priority = 0;
            ExtensionSettings = new Dictionary<string, object>();
            AllowedActions = new string[0];
        }
    }

    /// <summary>
    /// Agent action definition - what actions an agent can perform
    /// </summary>
    [Serializable]
    public class AgentAction
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] RequiredExtensions { get; set; }
        public bool IsEnabled { get; set; }
        public float Cooldown { get; set; }
        public Dictionary<string, object> ActionParameters { get; set; }

        public AgentAction(string name, string description = "")
        {
            Name = name;
            Description = description;
            RequiredExtensions = new string[0];
            IsEnabled = true;
            Cooldown = 0f;
            ActionParameters = new Dictionary<string, object>();
        }
    }
    #endregion

    #region Event System Models
    /// <summary>
    /// AI SDK Event data
    /// </summary>
    [Serializable]
    public class AISDKEvent
    {
        public string EventType { get; set; }
        public string Source { get; set; }
        public object Data { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public AISDKEvent(string eventType, string source, object data = null)
        {
            EventType = eventType;
            Source = source;
            Data = data;
            Timestamp = DateTime.Now;
            Metadata = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Agent event data
    /// </summary>
    [Serializable]
    public class AgentEvent : AISDKEvent
    {
        public string AgentId { get; set; }
        public AgentType AgentType { get; set; }
        public string Action { get; set; }
        public string Result { get; set; }

        public AgentEvent(string agentId, AgentType agentType, string action, string result = "") 
            : base("AgentAction", agentId)
        {
            AgentId = agentId;
            AgentType = agentType;
            Action = action;
            Result = result;
        }
    }

    /// <summary>
    /// Extension event data
    /// </summary>
    [Serializable]
    public class ExtensionEvent : AISDKEvent
    {
        public string ExtensionName { get; set; }
        public string Operation { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public ExtensionEvent(string extensionName, string operation, bool success, string errorMessage = "") 
            : base("ExtensionOperation", extensionName)
        {
            ExtensionName = extensionName;
            Operation = operation;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
    #endregion

    #region Extension Management Models
    /// <summary>
    /// Extension capability definition
    /// </summary>
    [Serializable]
    public class ExtensionCapability
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] SupportedActions { get; set; }
        public bool IsActive { get; set; }
        public float PerformanceScore { get; set; }
        public Dictionary<string, object> CapabilitySettings { get; set; }

        public ExtensionCapability(string name, string description = "")
        {
            Name = name;
            Description = description;
            SupportedActions = new string[0];
            IsActive = true;
            PerformanceScore = 1.0f;
            CapabilitySettings = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Extension registry entry
    /// </summary>
    [Serializable]
    public class ExtensionRegistryEntry
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public bool IsEnabled { get; set; }
        public int Priority { get; set; }
        public ExtensionCapability[] Capabilities { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public ExtensionRegistryEntry(string name, string version = "1.0.0")
        {
            Name = name;
            Version = version;
            Description = string.Empty;
            Tags = new string[0];
            IsEnabled = true;
            Priority = 0;
            Capabilities = new ExtensionCapability[0];
            Metadata = new Dictionary<string, object>();
        }
    }
    #endregion
}
