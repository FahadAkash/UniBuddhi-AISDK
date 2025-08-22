using System;
using System.Collections.Generic;
using UnityEngine;
using AISDK.Core.Interfaces;
using AISDK.Core.Models;
using AISDK.Core.TTS;

namespace AISDK.Core.Factories
{
    /// <summary>
    /// Factory for creating TTS providers
    /// </summary>
    public static class TTSProviderFactory
    {
        private static readonly Dictionary<TTSProviderType, Type> _ttsProviderTypes = new Dictionary<TTSProviderType, Type>();

        static TTSProviderFactory()
        {
            RegisterTTSProviders();
        }

        /// <summary>
        /// Register all available TTS providers
        /// </summary>
        private static void RegisterTTSProviders()
        {
            // Register built-in TTS providers
            RegisterTTSProvider(TTSProviderType.ElevenLabs, typeof(ElevenLabsProvider));
            RegisterTTSProvider(TTSProviderType.OpenAI, typeof(OpenAITTSProvider));
            RegisterTTSProvider(TTSProviderType.Azure, typeof(AzureTTSProvider));
            RegisterTTSProvider(TTSProviderType.Google, typeof(GoogleTTSProvider));
            RegisterTTSProvider(TTSProviderType.Amazon, typeof(AmazonTTSProvider));
        }

        /// <summary>
        /// Register a TTS provider type
        /// </summary>
        public static void RegisterTTSProvider(TTSProviderType providerType, Type providerClass)
        {
            if (typeof(ITTSProvider).IsAssignableFrom(providerClass))
            {
                _ttsProviderTypes[providerType] = providerClass;
                Debug.Log($"[AISDK] Registered TTS provider: {providerType} -> {providerClass.Name}");
            }
            else
            {
                Debug.LogError($"[AISDK] Failed to register TTS provider {providerType}: {providerClass.Name} does not implement ITTSProvider");
            }
        }

        /// <summary>
        /// Create a TTS provider instance
        /// </summary>
        public static ITTSProvider CreateProvider(TTSProviderType providerType, string apiKey = "", string voiceId = "", MonoBehaviour coroutineRunner = null)
        {
            try
            {
                if (!_ttsProviderTypes.ContainsKey(providerType))
                {
                    Debug.LogError($"[AISDK] TTS provider type {providerType} not registered");
                    return null;
                }

                var providerClass = _ttsProviderTypes[providerType];
                
                // Create GameObject and add component for MonoBehaviour-based providers
                GameObject go = new GameObject($"{providerType}TTSProvider");
                var provider = (ITTSProvider)go.AddComponent(providerClass);

                if (provider != null)
                {
                    var config = new TTSConfig(providerType, apiKey, voiceId);
                    provider.Initialize(config);
                    Debug.Log($"[AISDK] Created TTS provider: {providerType}");
                }

                return provider;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AISDK] Failed to create TTS provider {providerType}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a TTS provider with custom configuration
        /// </summary>
        public static ITTSProvider CreateProvider(TTSConfig config, MonoBehaviour coroutineRunner = null)
        {
            return CreateProvider(config.Type, config.ApiKey, config.VoiceId, coroutineRunner);
        }

        /// <summary>
        /// Get all registered TTS provider types
        /// </summary>
        public static List<TTSProviderType> GetRegisteredTTSProviders()
        {
            return new List<TTSProviderType>(_ttsProviderTypes.Keys);
        }

        /// <summary>
        /// Check if a TTS provider type is registered
        /// </summary>
        public static bool IsTTSProviderRegistered(TTSProviderType providerType)
        {
            return _ttsProviderTypes.ContainsKey(providerType);
        }

        /// <summary>
        /// Get TTS provider class type
        /// </summary>
        public static Type GetTTSProviderClass(TTSProviderType providerType)
        {
            return _ttsProviderTypes.ContainsKey(providerType) ? _ttsProviderTypes[providerType] : null;
        }

        /// <summary>
        /// Get default configuration for TTS provider type
        /// </summary>
        public static TTSConfig GetDefaultConfig(TTSProviderType providerType)
        {
            switch (providerType)
            {
                case TTSProviderType.ElevenLabs:
                    return new TTSConfig(providerType, "", "21m00Tcm4TlvDq8ikWAM") // Default ElevenLabs voice
                    {
                        BaseUrl = "https://api.elevenlabs.io",
                        Speed = 1.0f,
                        Pitch = 1.0f
                    };
                case TTSProviderType.OpenAI:
                    return new TTSConfig(providerType, "", "alloy") // Default OpenAI voice
                    {
                        BaseUrl = "https://api.openai.com",
                        Speed = 1.0f,
                        Pitch = 1.0f
                    };
                case TTSProviderType.Azure:
                    return new TTSConfig(providerType, "", "en-US-JennyNeural") // Default Azure voice
                    {
                        BaseUrl = "https://eastus.tts.speech.microsoft.com",
                        Speed = 1.0f,
                        Pitch = 1.0f
                    };
                case TTSProviderType.Google:
                    return new TTSConfig(providerType, "", "en-US-Standard-A") // Default Google voice
                    {
                        BaseUrl = "https://texttospeech.googleapis.com",
                        Speed = 1.0f,
                        Pitch = 1.0f
                    };
                case TTSProviderType.Amazon:
                    return new TTSConfig(providerType, "", "Joanna") // Default Amazon voice
                    {
                        BaseUrl = "https://polly.us-east-1.amazonaws.com",
                        Speed = 1.0f,
                        Pitch = 1.0f
                    };
                default:
                    return new TTSConfig(providerType, "", "");
            }
        }
    }
}
