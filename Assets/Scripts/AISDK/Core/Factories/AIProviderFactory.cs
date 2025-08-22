using System;
using System.Collections.Generic;
using UnityEngine;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;
using UniBuddhi.Core.Providers;

namespace UniBuddhi.Core.Factories
{
    /// <summary>
    /// Factory for creating AI providers
    /// </summary>
    public static class AIProviderFactory
    {
        private static readonly Dictionary<AIProviderType, Type> _providerTypes = new Dictionary<AIProviderType, Type>();

        static AIProviderFactory()
        {
            RegisterProviders();
        }

        /// <summary>
        /// Register all available providers
        /// </summary>
        private static void RegisterProviders()
        {
            try
            {
                // Register built-in providers
                RegisterProvider(AIProviderType.OpenAI, typeof(OpenAIProvider));
                RegisterProvider(AIProviderType.Gemini, typeof(GeminiProvider));
                RegisterProvider(AIProviderType.DeepSeek, typeof(DeepSeekProvider));
                RegisterProvider(AIProviderType.Anthropic, typeof(AnthropicProvider));
                RegisterProvider(AIProviderType.Cohere, typeof(CohereProvider));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AISDK] Error registering providers: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a provider type
        /// </summary>
        public static void RegisterProvider(AIProviderType providerType, Type providerClass)
        {
            if (typeof(IAIProvider).IsAssignableFrom(providerClass))
            {
                _providerTypes[providerType] = providerClass;
                Debug.Log($"[AISDK] Registered provider: {providerType} -> {providerClass.Name}");
            }
            else
            {
                Debug.LogError($"[AISDK] Failed to register provider {providerType}: {providerClass.Name} does not implement IAIProvider");
            }
        }

        /// <summary>
        /// Create a provider instance
        /// </summary>
        public static IAIProvider CreateProvider(AIProviderType providerType, string apiKey = "", string baseUrl = "", ModelType model = ModelType.GPT_3_5_Turbo, MonoBehaviour coroutineRunner = null)
        {
            try
            {
                if (!_providerTypes.ContainsKey(providerType))
                {
                    Debug.LogError($"[AISDK] Provider type {providerType} not registered");
                    return null;
                }

                var providerClass = _providerTypes[providerType];
                
                // Create GameObject for MonoBehaviour providers
                GameObject providerGO = new GameObject($"AIProvider_{providerType}");
                if (coroutineRunner != null)
                {
                    providerGO.transform.SetParent(coroutineRunner.transform);
                }
                
                var provider = (IAIProvider)providerGO.AddComponent(providerClass);

                if (provider != null)
                {
                    var config = new ProviderConfig(providerType, apiKey, baseUrl, model);
                    provider.Initialize(config);
                    Debug.Log($"[AISDK] Created provider: {providerType}");
                }

                return provider;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AISDK] Failed to create provider {providerType}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a provider with custom configuration
        /// </summary>
        public static IAIProvider CreateProvider(ProviderConfig config, MonoBehaviour coroutineRunner = null)
        {
            return CreateProvider(config.Type, config.ApiKey, config.BaseUrl, config.Model, coroutineRunner);
        }

        /// <summary>
        /// Get all registered provider types
        /// </summary>
        public static List<AIProviderType> GetRegisteredProviders()
        {
            return new List<AIProviderType>(_providerTypes.Keys);
        }

        /// <summary>
        /// Check if a provider type is registered
        /// </summary>
        public static bool IsProviderRegistered(AIProviderType providerType)
        {
            return _providerTypes.ContainsKey(providerType);
        }

        /// <summary>
        /// Get provider class type
        /// </summary>
        public static Type GetProviderClass(AIProviderType providerType)
        {
            return _providerTypes.ContainsKey(providerType) ? _providerTypes[providerType] : null;
        }
    }
}
