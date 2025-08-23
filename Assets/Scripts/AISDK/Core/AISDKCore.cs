using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;
using UniBuddhi.Core.Factories;
using UniBuddhi.Core.Extensions;
using UniBuddhi.Core.Agents;
using System.Linq;

namespace UniBuddhi.Core
{
    /// <summary>
    /// Main AI SDK Core - Singleton manager for all AI operations
    /// Supports multiple AI providers (OpenAI, Gemini, DeepSeek, etc.)
    /// </summary>
    public class AISDKCore : MonoBehaviour
    {
        #region Singleton
        private static AISDKCore _instance;
        public static AISDKCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AISDKCore>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AISDKCore");
                        _instance = go.AddComponent<AISDKCore>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Inspector Settings
        [Header("AI Provider Configuration")]
        [SerializeField] private AIProviderType selectedProvider = AIProviderType.Gemini;
        [SerializeField] private string apiKey = "";
        [SerializeField] private string baseUrl = "";
        
        [Header("Default Agent Configuration")]
        [SerializeField] private AgentType defaultAgentType = AgentType.Assistant;
        [SerializeField] private string defaultSystemPrompt = "You are a helpful AI assistant.";
        [SerializeField] private float defaultTemperature = 0.7f;
        [SerializeField] private int defaultMaxTokens = 1000;
        
        [Header("Extension System")]
        [SerializeField] private bool enableExtensions = true;
        [SerializeField] private List<MonoBehaviour> extensionBehaviours = new List<MonoBehaviour>();
        
        [Header("TTS Configuration")]
        [SerializeField] private bool enableTTS = true;
        [SerializeField] private TTSProviderType ttsProvider = TTSProviderType.ElevenLabs;
        [SerializeField] private string ttsApiKey = "";
        [SerializeField] private string ttsVoiceId = "";
        
        [Header("Function Calling System")]
        [SerializeField] private bool enableFunctionCalling = true;
        [SerializeField] private int maxFunctionCallsPerMessage = 5;
        [SerializeField] private bool autoDiscoverFunctionExtensions = true;
        [SerializeField] private List<MonoBehaviour> functionExtensionBehaviours = new List<MonoBehaviour>();
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool enablePerformanceMonitoring = false;
        #endregion

        #region Events
        // Core AI Events
        public static event Action<string> OnAIResponse;
        public static event Action<AudioClip> OnAudioGenerated;
        public static event Action<string> OnError;
        public static event Action OnSpeechStarted;
        public static event Action OnSpeechFinished;
        public static event Action<AIProviderType> OnProviderChanged;
        
        // Enhanced Event System
        public static event Action<AISDKEvent> OnSDKEvent;
        public static event Action<AgentEvent> OnAgentEvent;
        public static event Action<ExtensionEvent> OnExtensionEvent;
        
        // Agent Management Events
        public static event Action<IAgent> OnAgentCreated;
        public static event Action<IAgent> OnAgentDestroyed;
        public static event Action<string, string> OnAgentPersonalityChanged;
        public static event Action<string, string> OnAgentExtensionAdded;
        public static event Action<string, string> OnAgentExtensionRemoved;
        
        // Extension Management Events
        public static event Action<IAgentExtension> OnExtensionRegistered;
        public static event Action<IAgentExtension> OnExtensionUnregistered;
        public static event Action<string, string> OnExtensionCapabilityChanged;
        
        // System Events
        public static event Action OnSDKInitialized;
        public static event Action OnSDKShutdown;
        public static event Action<string> OnSystemMessage;
        #endregion

        #region Properties
        public AIProviderType CurrentProvider => selectedProvider;
        public bool IsInitialized => _isInitialized;
        public bool IsProcessing { get; private set; }
        public IAgent CurrentAgent => _agents.ContainsKey(defaultAgentType.ToString()) ? _agents[defaultAgentType.ToString()] : null;
        
        // Enhanced Properties
        public Dictionary<string, IAgent> AllAgents => new Dictionary<string, IAgent>(_agents);
        public List<IAgentExtension> AllExtensions => new List<IAgentExtension>(_extensions);
        public Dictionary<string, ExtensionRegistryEntry> ExtensionRegistry => new Dictionary<string, ExtensionRegistryEntry>(_extensionRegistry);
        
        // Function Calling Properties
        public FunctionCallManager FunctionManager => _functionCallManager;
        public bool FunctionCallingEnabled => _functionCallingEnabled && enableFunctionCalling;
        public List<IFunctionExtension> FunctionExtensions => new List<IFunctionExtension>(_functionExtensions);
        public Dictionary<string, EnhancedAgent> EnhancedAgents => new Dictionary<string, EnhancedAgent>(_enhancedAgents);
        #endregion

        #region Private Fields
        private Dictionary<AIProviderType, IAIProvider> _providers = new Dictionary<AIProviderType, IAIProvider>();
        private Dictionary<string, IAgent> _agents = new Dictionary<string, IAgent>();
        private List<IAgentExtension> _extensions = new List<IAgentExtension>();
        private Dictionary<string, ExtensionRegistryEntry> _extensionRegistry = new Dictionary<string, ExtensionRegistryEntry>();
        private Dictionary<string, List<string>> _agentExtensions = new Dictionary<string, List<string>>();
        private Dictionary<string, AgentPersonality> _agentPersonalities = new Dictionary<string, AgentPersonality>();
        private ITTSProvider _ttsProvider;
        private bool _isInitialized = false;
        
        // Enhanced Function Calling Support
        private FunctionCallManager _functionCallManager;
        private Dictionary<string, EnhancedAgent> _enhancedAgents = new Dictionary<string, EnhancedAgent>();
        private List<IFunctionExtension> _functionExtensions = new List<IFunctionExtension>();
        private bool _functionCallingEnabled = true;
        #endregion

        #region Enhanced Agent Management
        /// <summary>
        /// Create an agent with custom personality and extensions
        /// </summary>
        public IAgent CreateAgent(string agentId, AgentType agentType, AgentPersonality personality = null, string[] extensionNames = null)
        {
            if (_agents.ContainsKey(agentId))
            {
                LogWarning($"Agent {agentId} already exists!");
                return _agents[agentId];
            }

            try
            {
                var config = new EnhancedAgentConfig(agentType, personality);
                if (extensionNames != null)
                {
                    config.ExtensionNames.AddRange(extensionNames);
                }

                var agent = AgentFactory.CreateAgent(config, GetCurrentProvider());
                if (agent != null)
                {
                    _agents[agentId] = agent;
                    _agentPersonalities[agentId] = personality ?? new AgentPersonality(agentType.ToString());
                    _agentExtensions[agentId] = new List<string>(config.ExtensionNames);

                    // Bind extensions to agent
                    foreach (var extName in config.ExtensionNames)
                    {
                        BindExtensionToAgent(agentId, extName);
                    }

                    OnAgentCreated?.Invoke(agent);
                    BroadcastEvent(new AgentEvent(agentId, agentType, "Created", "Agent created successfully"));
                    
                    LogDebug($"Created agent {agentId} with {config.ExtensionNames.Count} extensions");
                    return agent;
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to create agent {agentId}: {ex.Message}");
                OnError?.Invoke($"Failed to create agent: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Set agent personality
        /// </summary>
        public void SetAgentPersonality(string agentId, AgentPersonality personality)
        {
            if (!_agents.ContainsKey(agentId))
            {
                LogWarning($"Agent {agentId} not found!");
                return;
            }

            _agentPersonalities[agentId] = personality;
            var agent = _agents[agentId];
            
            if (agent is BaseAgent baseAgent)
            {
                baseAgent.SetSystemPrompt(personality.SystemPrompt);
            }

            OnAgentPersonalityChanged?.Invoke(agentId, personality.Name);
            BroadcastEvent(new AgentEvent(agentId, agent.Type, "PersonalityChanged", personality.Name));
            
            LogDebug($"Updated personality for agent {agentId}: {personality.Name}");
        }

        /// <summary>
        /// Add extension to agent
        /// </summary>
        public bool AddExtensionToAgent(string agentId, string extensionName)
        {
            if (!_agents.ContainsKey(agentId))
            {
                LogWarning($"Agent {agentId} not found!");
                return false;
            }

            if (!_extensionRegistry.ContainsKey(extensionName))
            {
                LogWarning($"Extension {extensionName} not registered!");
                return false;
            }

            if (!_agentExtensions.ContainsKey(agentId))
            {
                _agentExtensions[agentId] = new List<string>();
            }

            if (!_agentExtensions[agentId].Contains(extensionName))
            {
                _agentExtensions[agentId].Add(extensionName);
                OnAgentExtensionAdded?.Invoke(agentId, extensionName);
                BroadcastEvent(new AgentEvent(agentId, _agents[agentId].Type, "ExtensionAdded", extensionName));
                
                LogDebug($"Added extension {extensionName} to agent {agentId}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove extension from agent
        /// </summary>
        public bool RemoveExtensionFromAgent(string agentId, string extensionName)
        {
            if (_agentExtensions.ContainsKey(agentId) && _agentExtensions[agentId].Contains(extensionName))
            {
                _agentExtensions[agentId].Remove(extensionName);
                OnAgentExtensionRemoved?.Invoke(agentId, extensionName);
                BroadcastEvent(new AgentEvent(agentId, _agents[agentId].Type, "ExtensionRemoved", extensionName));
                
                LogDebug($"Removed extension {extensionName} from agent {agentId}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get agent extensions
        /// </summary>
        public List<string> GetAgentExtensions(string agentId)
        {
            return _agentExtensions.ContainsKey(agentId) ? new List<string>(_agentExtensions[agentId]) : new List<string>();
        }

        /// <summary>
        /// Get agent personality
        /// </summary>
        public AgentPersonality GetAgentPersonality(string agentId)
        {
            return _agentPersonalities.ContainsKey(agentId) ? _agentPersonalities[agentId] : null;
        }

        /// <summary>
        /// Check if agent can perform action
        /// </summary>
        public bool CanAgentPerformAction(string agentId, string actionName)
        {
            if (!_agents.ContainsKey(agentId) || !_agentExtensions.ContainsKey(agentId))
                return false;

            var agent = _agents[agentId];
            var extensions = _agentExtensions[agentId];

            // Check if agent has required extensions for the action
            foreach (var extName in extensions)
            {
                if (_extensionRegistry.ContainsKey(extName))
                {
                    var ext = _extensionRegistry[extName];
                    if (ext.Capabilities.Any(c => c.SupportedActions.Contains(actionName)))
                        return true;
                }
            }

            return false;
        }
        #endregion

        #region Extension Management
        /// <summary>
        /// Register an extension with capabilities
        /// </summary>
        public void RegisterExtension(IAgentExtension extension, ExtensionCapability[] capabilities = null)
        {
            if (extension == null) return;

            var entry = new ExtensionRegistryEntry(extension.Name, extension.Version)
            {
                Description = extension.Description,
                Capabilities = capabilities ?? new ExtensionCapability[0]
            };

            _extensionRegistry[extension.Name] = entry;
            _extensions.Add(extension);

            OnExtensionRegistered?.Invoke(extension);
            BroadcastEvent(new ExtensionEvent(extension.Name, "Registered", true));
            
            LogDebug($"Registered extension: {extension.Name} with {capabilities?.Length ?? 0} capabilities");
        }

        /// <summary>
        /// Unregister an extension
        /// </summary>
        public void UnregisterExtension(string extensionName)
        {
            if (_extensionRegistry.ContainsKey(extensionName))
            {
                var extension = _extensions.FirstOrDefault(e => e.Name == extensionName);
                if (extension != null)
                {
                    _extensions.Remove(extension);
                    OnExtensionUnregistered?.Invoke(extension);
                    BroadcastEvent(new ExtensionEvent(extensionName, "Unregistered", true));
                }

                _extensionRegistry.Remove(extensionName);
                
                // Remove from all agents
                foreach (var agentId in _agentExtensions.Keys.ToList())
                {
                    RemoveExtensionFromAgent(agentId, extensionName);
                }

                LogDebug($"Unregistered extension: {extensionName}");
            }
        }

        /// <summary>
        /// Get extension capabilities
        /// </summary>
        public ExtensionCapability[] GetExtensionCapabilities(string extensionName)
        {
            return _extensionRegistry.ContainsKey(extensionName) ? _extensionRegistry[extensionName].Capabilities : new ExtensionCapability[0];
        }
        #endregion

        #region Event Broadcasting
        /// <summary>
        /// Broadcast SDK event
        /// </summary>
        public void BroadcastEvent(AISDKEvent sdkEvent)
        {
            OnSDKEvent?.Invoke(sdkEvent);
            
            // Route to specific event handlers
            if (sdkEvent is AgentEvent agentEvent)
            {
                OnAgentEvent?.Invoke(agentEvent);
            }
            else if (sdkEvent is ExtensionEvent extensionEvent)
            {
                OnExtensionEvent?.Invoke(extensionEvent);
            }
        }

        /// <summary>
        /// Send system message
        /// </summary>
        public void SendSystemMessage(string message)
        {
            OnSystemMessage?.Invoke(message);
            LogDebug($"[SYSTEM] {message}");
        }
        #endregion

        #region Private Methods
        private void BindExtensionToAgent(string agentId, string extensionName)
        {
            if (!_agentExtensions.ContainsKey(agentId))
            {
                _agentExtensions[agentId] = new List<string>();
            }

            if (!_agentExtensions[agentId].Contains(extensionName))
            {
                _agentExtensions[agentId].Add(extensionName);
            }
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSDK();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (!_isInitialized)
            {
                InitializeSDK();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        #endregion

        #region Initialization
        private void InitializeSDK()
        {
            if (_isInitialized) return;

            try
            {
                LogDebug("Initializing AI SDK...");
                
                // Initialize function call manager
                InitializeFunctionCallManager();
                
                // Initialize providers
                InitializeProviders();
                
                // Initialize agents
                InitializeAgents();
                
                // Initialize extensions
                if (enableExtensions)
                {
                    InitializeExtensions();
                }
                
                // Initialize function extensions
                if (enableFunctionCalling)
                {
                    InitializeFunctionExtensions();
                }
                
                // Initialize TTS
                if (enableTTS)
                {
                    InitializeTTS();
                }
                
                // Initialize enhanced agents
                InitializeEnhancedAgents();
                
                _isInitialized = true;
                OnSDKInitialized?.Invoke();
                LogDebug("AI SDK initialized successfully!");
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize AI SDK: {ex.Message}");
                _isInitialized = false;
            }
        }

        private void InitializeProviders()
        {
            LogDebug("Initializing AI providers...");
            
            // Initialize all supported providers
            var providerTypes = Enum.GetValues(typeof(AIProviderType));
            foreach (AIProviderType providerType in providerTypes)
            {
                try
                {
                    var provider = AIProviderFactory.CreateProvider(providerType, apiKey, baseUrl, ModelType.GPT_3_5_Turbo, this);
                    if (provider != null)
                    {
                        _providers[providerType] = provider;
                        LogDebug($"Initialized provider: {providerType}");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to initialize provider {providerType}: {ex.Message}");
                }
            }
        }

        private void InitializeAgents()
        {
            LogDebug("Initializing agents...");
            
            var agentTypes = Enum.GetValues(typeof(AgentType));
            foreach (AgentType agentType in agentTypes)
            {
                try
                {
                    var config = new AgentConfig(
                        agentType,
                        GetDefaultSystemPrompt(agentType),
                        defaultTemperature,
                        defaultMaxTokens
                    );
                    
                    var agent = AgentFactory.CreateAgent(config, GetCurrentProvider());
                    if (agent != null)
                    {
                        _agents[agentType.ToString()] = agent;
                        LogDebug($"Initialized agent: {agentType}");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to initialize agent {agentType}: {ex.Message}");
                }
            }
        }

        private void InitializeExtensions()
        {
            LogDebug("Initializing extensions...");
            
            // Collect from assigned behaviours
            foreach (var mb in extensionBehaviours)
            {
                if (mb is IAgentExtension ext)
                {
                    _extensions.Add(ext);
                    LogDebug($"Loaded extension: {ext.Name}");
                }
            }
            
            // Collect from same GameObject
            var local = GetComponents<MonoBehaviour>();
            foreach (var mb in local)
            {
                if (mb is IAgentExtension ext && !_extensions.Contains(ext))
                {
                    _extensions.Add(ext);
                    LogDebug($"Found local extension: {ext.Name}");
                }
            }
        }

        private void InitializeTTS()
        {
            LogDebug("Initializing TTS provider...");
            
            try
            {
                _ttsProvider = TTSProviderFactory.CreateProvider(ttsProvider, ttsApiKey, ttsVoiceId, this);
                LogDebug($"Initialized TTS provider: {ttsProvider}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize TTS provider: {ex.Message}");
            }
        }

        private void InitializeFunctionCallManager()
        {
            LogDebug("Initializing Function Call Manager...");
            
            // Create function call manager if it doesn't exist
            _functionCallManager = GetComponent<FunctionCallManager>();
            if (_functionCallManager == null)
            {
                _functionCallManager = gameObject.AddComponent<FunctionCallManager>();
            }
            
            _functionCallManager.Initialize();
            LogDebug("Function Call Manager initialized");
        }

        private void InitializeFunctionExtensions()
        {
            LogDebug("Initializing Function Extensions...");
            
            // Collect from assigned function extension behaviours
            foreach (var mb in functionExtensionBehaviours)
            {
                if (mb is IFunctionExtension funcExt)
                {
                    AddFunctionExtension(funcExt);
                }
            }
            
            // Auto-discover function extensions if enabled
            if (autoDiscoverFunctionExtensions)
            {
                DiscoverFunctionExtensions();
            }
            
            LogDebug($"Initialized {_functionExtensions.Count} function extensions");
        }

        private void DiscoverFunctionExtensions()
        {
            // Search in current GameObject
            var localComponents = GetComponents<MonoBehaviour>();
            foreach (var component in localComponents)
            {
                if (component is IFunctionExtension funcExt && !_functionExtensions.Contains(funcExt))
                {
                    AddFunctionExtension(funcExt);
                }
            }
            
            // Search in child GameObjects
            var childComponents = GetComponentsInChildren<MonoBehaviour>();
            foreach (var component in childComponents)
            {
                if (component is IFunctionExtension funcExt && !_functionExtensions.Contains(funcExt))
                {
                    AddFunctionExtension(funcExt);
                }
            }
        }

        private void InitializeEnhancedAgents()
        {
            LogDebug("Initializing Enhanced Agents...");
            
            if (!FunctionCallingEnabled)
            {
                LogDebug("Function calling disabled, skipping enhanced agents");
                return;
            }
            
            // Create enhanced versions of default agents
            var agentTypes = Enum.GetValues(typeof(AgentType));
            foreach (AgentType agentType in agentTypes)
            {
                if (agentType == AgentType.Custom) continue;
                
                try
                {
                    CreateEnhancedAgent($"Enhanced{agentType}", agentType);
                }
                catch (Exception ex)
                {
                    LogError($"Failed to create enhanced agent {agentType}: {ex.Message}");
                }
            }
            
            LogDebug($"Initialized {_enhancedAgents.Count} enhanced agents");
        }
        #endregion

        #region Public API
        /// <summary>
        /// Send a message to the AI and optionally generate speech
        /// </summary>
        public void SendMessage(string message, AgentType agentType = AgentType.Assistant, bool generateSpeech = true)
        {
            if (!_isInitialized)
            {
                LogError("AI SDK not initialized!");
                OnError?.Invoke("AI SDK not initialized");
                return;
            }

            if (IsProcessing)
            {
                LogWarning("Already processing a request, please wait...");
                return;
            }

            StartCoroutine(ProcessMessageCoroutine(message, agentType, generateSpeech));
        }

        /// <summary>
        /// Stream a message from the AI
        /// </summary>
        public void StreamMessage(string message, AgentType agentType = AgentType.Assistant, Action<string, bool> onChunk = null)
        {
            if (!_isInitialized)
            {
                LogError("AI SDK not initialized!");
                OnError?.Invoke("AI SDK not initialized");
                return;
            }

            if (IsProcessing)
            {
                LogWarning("Already processing a request, please wait...");
                return;
            }

            StartCoroutine(StreamMessageCoroutine(message, agentType, onChunk));
        }

        /// <summary>
        /// Change the current AI provider
        /// </summary>
        public void SetProvider(AIProviderType providerType)
        {
            if (_providers.ContainsKey(providerType))
            {
                selectedProvider = providerType;
                OnProviderChanged?.Invoke(providerType);
                LogDebug($"Switched to provider: {providerType}");
            }
            else
            {
                LogError($"Provider {providerType} not available!");
            }
        }

        /// <summary>
        /// Set the API key for the current provider
        /// </summary>
        public void SetApiKey(string key)
        {
            apiKey = key;
            if (_providers.ContainsKey(selectedProvider))
            {
                _providers[selectedProvider].SetApiKey(key);
            }
        }

        /// <summary>
        /// Add an extension to the system
        /// </summary>
        public void AddExtension(IAgentExtension extension)
        {
            if (extension != null && !_extensions.Contains(extension))
            {
                _extensions.Add(extension);
                LogDebug($"Added extension: {extension.Name}");
            }
        }

        /// <summary>
        /// Remove an extension from the system
        /// </summary>
        public void RemoveExtension(IAgentExtension extension)
        {
            if (_extensions.Contains(extension))
            {
                _extensions.Remove(extension);
                LogDebug($"Removed extension: {extension.Name}");
            }
        }

        /// <summary>
        /// Get all available extensions
        /// </summary>
        public List<IAgentExtension> GetExtensions()
        {
            return new List<IAgentExtension>(_extensions);
        }

        /// <summary>
        /// Stop current audio playback
        /// </summary>
        public void StopAudio()
        {
            _ttsProvider?.StopAudio();
        }

        /// <summary>
        /// Set audio volume
        /// </summary>
        public void SetVolume(float volume)
        {
            _ttsProvider?.SetVolume(volume);
        }
        #endregion

        #region Enhanced Public API - Function Calling
        /// <summary>
        /// Create an enhanced agent with function calling capabilities
        /// </summary>
        public EnhancedAgent CreateEnhancedAgent(string agentId, AgentType agentType, AgentPersonality personality = null, string[] functionExtensions = null)
        {
            if (_enhancedAgents.ContainsKey(agentId))
            {
                LogWarning($"Enhanced agent {agentId} already exists!");
                return _enhancedAgents[agentId];
            }

            try
            {
                var config = new EnhancedAgentConfig(agentType, personality);
                if (functionExtensions != null)
                {
                    config.ExtensionNames.AddRange(functionExtensions);
                }

                var agent = new EnhancedAgent(agentType);
                agent.Initialize(config, GetCurrentProvider());
                
                // Add requested function extensions
                if (functionExtensions != null)
                {
                    foreach (var extName in functionExtensions)
                    {
                        var extension = _functionExtensions.FirstOrDefault(e => e.Name == extName);
                        if (extension != null)
                        {
                            agent.AddFunctionExtension(extension);
                        }
                    }
                }
                
                _enhancedAgents[agentId] = agent;
                
                OnAgentCreated?.Invoke(agent);
                BroadcastEvent(new AgentEvent(agentId, agentType, "Created", "Enhanced agent created successfully"));
                
                LogDebug($"Created enhanced agent {agentId} with {agent.FunctionExtensions.Count} function extensions");
                return agent;
            }
            catch (Exception ex)
            {
                LogError($"Failed to create enhanced agent {agentId}: {ex.Message}");
                OnError?.Invoke($"Failed to create enhanced agent: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Send message to an enhanced agent with function calling support
        /// </summary>
        public void SendMessageToEnhancedAgent(string agentId, string message, bool generateSpeech = true)
        {
            if (!_enhancedAgents.ContainsKey(agentId))
            {
                LogError($"Enhanced agent {agentId} not found!");
                OnError?.Invoke($"Enhanced agent {agentId} not found");
                return;
            }

            if (IsProcessing)
            {
                LogWarning("Already processing a request, please wait...");
                return;
            }

            StartCoroutine(ProcessEnhancedMessageCoroutine(agentId, message, generateSpeech));
        }

        /// <summary>
        /// Add a function extension to the system
        /// </summary>
        public void AddFunctionExtension(IFunctionExtension extension)
        {
            if (extension == null)
            {
                LogWarning("Cannot add null function extension");
                return;
            }

            if (!_functionExtensions.Contains(extension))
            {
                _functionExtensions.Add(extension);
                
                // Register with function call manager
                if (_functionCallManager != null)
                {
                    _functionCallManager.RegisterExtension(extension);
                }
                
                LogDebug($"Added function extension: {extension.Name}");
            }
        }

        /// <summary>
        /// Remove a function extension from the system
        /// </summary>
        public void RemoveFunctionExtension(IFunctionExtension extension)
        {
            if (extension != null && _functionExtensions.Contains(extension))
            {
                _functionExtensions.Remove(extension);
                
                // Unregister from function call manager
                if (_functionCallManager != null)
                {
                    _functionCallManager.UnregisterExtension(extension);
                }
                
                // Remove from all enhanced agents
                foreach (var agent in _enhancedAgents.Values)
                {
                    agent.RemoveFunctionExtension(extension);
                }
                
                LogDebug($"Removed function extension: {extension.Name}");
            }
        }

        /// <summary>
        /// Execute a function call directly
        /// </summary>
        public void ExecuteFunction(string functionName, string arguments, Action<FunctionResult> onComplete)
        {
            if (_functionCallManager == null)
            {
                LogError("Function call manager not initialized");
                onComplete?.Invoke(new FunctionResult(functionName, "", false, "", "Function call manager not initialized"));
                return;
            }

            var functionCall = new FunctionCall(functionName, arguments);
            StartCoroutine(_functionCallManager.ExecuteFunctionAsync(functionCall, onComplete));
        }

        /// <summary>
        /// Get all available function definitions
        /// </summary>
        public List<FunctionDefinition> GetAvailableFunctions()
        {
            return _functionCallManager?.AvailableFunctions ?? new List<FunctionDefinition>();
        }

        /// <summary>
        /// Get enhanced agent by ID
        /// </summary>
        public EnhancedAgent GetEnhancedAgent(string agentId)
        {
            return _enhancedAgents.ContainsKey(agentId) ? _enhancedAgents[agentId] : null;
        }

        /// <summary>
        /// Create a personality for agents
        /// </summary>
        public AgentPersonality CreatePersonality(string name, string systemPrompt, Dictionary<string, float> traits = null, List<string> capabilities = null)
        {
            var personality = new AgentPersonality(name, systemPrompt)
            {
                Traits = traits ?? new Dictionary<string, float>(),
                Capabilities = capabilities ?? new List<string>()
            };
            
            LogDebug($"Created personality: {name}");
            return personality;
        }

        /// <summary>
        /// Get system status including function calling information
        /// </summary>
        public string GetEnhancedSystemStatus()
        {
            var status = $"UniBuddhi AI SDK Enhanced Status:\n";
            status += $"- SDK Initialized: {IsInitialized}\n";
            status += $"- Current Provider: {CurrentProvider}\n";
            status += $"- Function Calling: {FunctionCallingEnabled}\n";
            status += $"- Standard Agents: {_agents.Count}\n";
            status += $"- Enhanced Agents: {_enhancedAgents.Count}\n";
            status += $"- Function Extensions: {_functionExtensions.Count}\n";
            status += $"- Available Functions: {GetAvailableFunctions().Count}\n";
            status += $"- TTS Enabled: {enableTTS}\n";
            
            if (_functionCallManager != null)
            {
                var stats = _functionCallManager.GetStatistics();
                status += $"- Total Function Calls: {stats["total_calls_executed"]}\n";
                status += $"- Function Success Rate: {stats["success_rate"]:F1}%\n";
            }
            
            return status;
        }
        #endregion

        #region Private Methods
        private System.Collections.IEnumerator ProcessMessageCoroutine(string message, AgentType agentType, bool generateSpeech)
        {
            IsProcessing = true;
            
            // Step 1: Extension preprocessing
            string combinedContext = "";
            if (enableExtensions && _extensions.Count > 0)
            {
                bool preprocessingComplete = false;
                bool preprocessingError = false;
                string errorMessage = "";

                StartCoroutine(BuildExtensionContext(message, (context) => 
                {
                    combinedContext = context;
                    preprocessingComplete = true;
                }));

                while (!preprocessingComplete)
                {
                    yield return null;
                }

                if (preprocessingError)
                {
                    LogError($"Error in extension preprocessing: {errorMessage}");
                    OnError?.Invoke($"Extension Error: {errorMessage}");
                    IsProcessing = false;
                    yield break;
                }
            }

            // Step 2: Get AI response
            string aiResponse = "";
            if (_agents.ContainsKey(agentType.ToString()))
            {
                if (!string.IsNullOrEmpty(combinedContext))
                {
                    _agents[agentType.ToString()].SetContext(combinedContext);
                }
                
                bool aiResponseComplete = false;
                bool aiResponseError = false;
                string errorMessage = "";

                StartCoroutine(GetAIResponse(message, agentType, (response) => 
                {
                    aiResponse = response;
                    aiResponseComplete = true;
                }));

                while (!aiResponseComplete)
                {
                    yield return null;
                }

                if (aiResponseError)
                {
                    LogError($"Error getting AI response: {errorMessage}");
                    OnError?.Invoke($"AI Error: {errorMessage}");
                    IsProcessing = false;
                    yield break;
                }
            }

            if (string.IsNullOrEmpty(aiResponse))
            {
                LogError("No response from AI");
                OnError?.Invoke("No response from AI");
                IsProcessing = false;
                yield break;
            }

            // Step 3: Extension postprocessing
            string finalResponse = aiResponse;
            if (enableExtensions && _extensions.Count > 0)
            {
                bool postprocessingComplete = false;

                StartCoroutine(ApplyExtensionPostprocessing(aiResponse, (response) => 
                {
                    finalResponse = response;
                    postprocessingComplete = true;
                }));

                while (!postprocessingComplete)
                {
                    yield return null;
                }
            }

            // Step 4: Generate speech if requested
            if (generateSpeech && _ttsProvider != null)
            {
                bool speechComplete = false;

                StartCoroutine(GenerateSpeech(finalResponse));

                while (!speechComplete)
                {
                    yield return null;
                }
            }

            // Step 5: Clear context and finish
            try
            {
                _agents[agentType.ToString()]?.ClearContext();
                OnAIResponse?.Invoke(finalResponse);
            }
            catch (Exception ex)
            {
                LogError($"Error finalizing process: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private System.Collections.IEnumerator StreamMessageCoroutine(string message, AgentType agentType, Action<string, bool> onChunk)
        {
            IsProcessing = true;
            
            if (_agents.ContainsKey(agentType.ToString()))
            {
                bool streamComplete = false;
                bool streamError = false;
                string errorMessage = "";

                StartCoroutine(_agents[agentType.ToString()].StreamAsync(message, (chunk, isComplete) =>
                {
                    if (isComplete)
                    {
                        streamComplete = true;
                    }
                    else
                    {
                        onChunk?.Invoke(chunk, isComplete);
                    }
                }));

                while (!streamComplete)
                {
                    yield return null;
                }

                if (streamError)
                {
                    LogError($"Error streaming message: {errorMessage}");
                    OnError?.Invoke($"Error: {errorMessage}");
                }
            }
            else
            {
                LogError($"Agent {agentType} not found");
                OnError?.Invoke($"Agent {agentType} not found");
            }
            
            IsProcessing = false;
        }

        private System.Collections.IEnumerator BuildExtensionContext(string message, Action<string> onComplete)
        {
            string combinedContext = "";
            var usedExtensions = new List<string>();

            foreach (var ext in _extensions)
            {
                ExtensionContext ctx = null;
                yield return ext.Preprocess(message, (c) => ctx = c);
                if (ctx != null && !string.IsNullOrEmpty(ctx.Context))
                {
                    combinedContext += (combinedContext.Length > 0 ? "\n\n" : "") + ctx.Context;
                    usedExtensions.Add(ext.Name);
                }
            }

            LogDebug($"Built context from extensions: {string.Join(", ", usedExtensions)}");
            onComplete?.Invoke(combinedContext);
        }

        private System.Collections.IEnumerator GetAIResponse(string message, AgentType agentType, Action<string> onComplete)
        {
            string response = "";
            bool complete = false;

            _agents[agentType.ToString()].ChatAsync(message, (result) =>
            {
                if (result.Success)
                {
                    response = result.Content;
                }
                else
                {
                    LogError($"AI Error: {result.Error}");
                    OnError?.Invoke($"AI Error: {result.Error}");
                }
                complete = true;
            });

            while (!complete)
            {
                yield return null;
            }

            onComplete?.Invoke(response);
        }

        private System.Collections.IEnumerator ApplyExtensionPostprocessing(string response, Action<string> onComplete)
        {
            string finalResponse = response;

            foreach (var ext in _extensions)
            {
                ExtensionResult result = null;
                yield return ext.Postprocess(finalResponse, (r) => result = r);
                if (result != null && !string.IsNullOrEmpty(result.ModifiedText))
                {
                    finalResponse = result.ModifiedText;
                }
            }

            onComplete?.Invoke(finalResponse);
        }

        private System.Collections.IEnumerator GenerateSpeech(string text)
        {
            if (_ttsProvider == null)
            {
                LogWarning("TTS provider not available");
                yield break;
            }

            AudioClip clip = null;
            bool complete = false;

            _ttsProvider.GenerateSpeech(text, (audioClip) =>
            {
                clip = audioClip;
                complete = true;
            });

            while (!complete)
            {
                yield return null;
            }

            if (clip != null)
            {
                OnAudioGenerated?.Invoke(clip);
                _ttsProvider.PlayAudio(clip);
            }
        }

        private IAIProvider GetCurrentProvider()
        {
            return _providers.ContainsKey(selectedProvider) ? _providers[selectedProvider] : null;
        }

        private string GetDefaultSystemPrompt(AgentType agentType)
        {
            switch (agentType)
            {
                case AgentType.Assistant:
                    return "You are a helpful AI assistant. Provide clear, accurate, and helpful responses.";
                case AgentType.Creative:
                    return "You are a creative AI focused on imagination, storytelling, and artistic expression.";
                case AgentType.Technical:
                    return "You are a technical AI expert. Provide precise, detailed technical information.";
                case AgentType.Analytical:
                    return "You are an analytical AI focused on data analysis and problem-solving.";
                case AgentType.Conversational:
                    return "You are a friendly conversational AI. Be engaging and personable.";
                default:
                    return defaultSystemPrompt;
            }
        }

        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[AISDK] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"[AISDK] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[AISDK] {message}");
        }

        #region Enhanced Message Processing
        /// <summary>
        /// Process message with enhanced agent and function calling
        /// </summary>
        private System.Collections.IEnumerator ProcessEnhancedMessageCoroutine(string agentId, string message, bool generateSpeech)
        {
            IsProcessing = true;
            
            // Check if agent exists
            if (!_enhancedAgents.ContainsKey(agentId))
            {
                LogError($"Enhanced agent not found: {agentId}");
                OnError?.Invoke($"Enhanced agent not found: {agentId}");
                IsProcessing = false;
                yield break;
            }
            
            yield return StartCoroutine(ProcessEnhancedMessageInternal(agentId, message, generateSpeech));
            IsProcessing = false;
        }
        
        private System.Collections.IEnumerator ProcessEnhancedMessageInternal(string agentId, string message, bool generateSpeech)
        {
            // Check if agent exists first
            if (!_enhancedAgents.ContainsKey(agentId))
            {
                LogError($"Enhanced agent not found: {agentId}");
                OnError?.Invoke($"Enhanced agent not found: {agentId}");
                yield break;
            }
            
            EnhancedAgent agent = null;
            string errorMessage = null;
            
            // Safe agent retrieval
            try
            {
                agent = _enhancedAgents[agentId];
                LogDebug($"Processing message with enhanced agent: {agentId}");
            }
            catch (Exception ex)
            {
                errorMessage = $"Error retrieving enhanced agent: {ex.Message}";
            }
            
            // Handle agent retrieval errors
            if (!string.IsNullOrEmpty(errorMessage) || agent == null)
            {
                LogError(errorMessage ?? "Failed to retrieve agent");
                OnError?.Invoke($"Enhanced agent error: {errorMessage ?? "Failed to retrieve agent"}");
                yield break;
            }
            
            // Execute the coroutine logic without try-catch around yield
            yield return StartCoroutine(ProcessEnhancedMessageCore(agent, agentId, message, generateSpeech));
        }
        
        private System.Collections.IEnumerator ProcessEnhancedMessageCore(EnhancedAgent agent, string agentId, string message, bool generateSpeech)
        {
            string response = "";
            
            // Send message to enhanced agent (handles function calling internally)
            bool responseComplete = false;
            
            agent.ChatAsync(message, (result) =>
            {
                if (result.Success)
                {
                    response = result.Content;
                }
                else
                {
                    LogError($"Enhanced agent error: {result.Error}");
                    OnError?.Invoke($"Enhanced agent error: {result.Error}");
                }
                responseComplete = true;
            });
            
            while (!responseComplete)
            {
                yield return null;
            }
            
            if (string.IsNullOrEmpty(response))
            {
                LogError("No response from enhanced agent");
                OnError?.Invoke("No response from enhanced agent");
                yield break;
            }
            
            // Generate speech if requested
            if (generateSpeech && _ttsProvider != null)
            {
                yield return GenerateSpeech(response);
            }
            
            // Broadcast response
            OnAIResponse?.Invoke(response);
            
            LogDebug($"Enhanced agent {agentId} completed message processing");
        }
        #endregion
        #endregion
    }
}
