using System;
using System.Collections.Generic;
using UnityEngine;
using AISDK.Core.Interfaces;
using AISDK.Core.Models;
using AISDK.Core.Factories;
using AISDK.Core.Extensions;

namespace AISDK.Core
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
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool enablePerformanceMonitoring = false;
        #endregion

        #region Private Fields
        private Dictionary<AIProviderType, IAIProvider> _providers = new Dictionary<AIProviderType, IAIProvider>();
        private Dictionary<AgentType, IAgent> _agents = new Dictionary<AgentType, IAgent>();
        private List<IAgentExtension> _extensions = new List<IAgentExtension>();
        private ITTSProvider _ttsProvider;
        private bool _isInitialized = false;
        #endregion

        #region Events
        public static event Action<string> OnAIResponse;
        public static event Action<AudioClip> OnAudioGenerated;
        public static event Action<string> OnError;
        public static event Action OnSpeechStarted;
        public static event Action OnSpeechFinished;
        public static event Action<AIProviderType> OnProviderChanged;
        #endregion

        #region Properties
        public AIProviderType CurrentProvider => selectedProvider;
        public bool IsInitialized => _isInitialized;
        public bool IsProcessing { get; private set; }
        public IAgent CurrentAgent => _agents.ContainsKey(defaultAgentType) ? _agents[defaultAgentType] : null;
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
                
                // Initialize providers
                InitializeProviders();
                
                // Initialize agents
                InitializeAgents();
                
                // Initialize extensions
                if (enableExtensions)
                {
                    InitializeExtensions();
                }
                
                // Initialize TTS
                if (enableTTS)
                {
                    InitializeTTS();
                }
                
                _isInitialized = true;
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
                        _agents[agentType] = agent;
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
            if (_agents.ContainsKey(agentType))
            {
                if (!string.IsNullOrEmpty(combinedContext))
                {
                    _agents[agentType].SetContext(combinedContext);
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
                _agents[agentType]?.ClearContext();
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
            
            if (_agents.ContainsKey(agentType))
            {
                bool streamComplete = false;
                bool streamError = false;
                string errorMessage = "";

                StartCoroutine(_agents[agentType].StreamAsync(message, (chunk, isComplete) =>
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

            _agents[agentType].ChatAsync(message, (result) =>
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
        #endregion
    }
}
