using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AISDK.Core.Interfaces;
using AISDK.Core.Models;

namespace AISDK.Core.Providers
{
    /// <summary>
    /// Cohere provider implementation
    /// </summary>
    public class CohereProvider : MonoBehaviour, IAIProvider
    {
        #region Properties
        public AIProviderType Type => AIProviderType.Cohere;
        public string Name => "Cohere";
        public bool IsInitialized { get; private set; }
        public string ApiKey { get; private set; }
        public string BaseUrl { get; private set; }
        public ModelType CurrentModel { get; private set; }
        #endregion

        #region Private Fields
        private ProviderConfig _config;
        private Dictionary<string, object> _statistics = new Dictionary<string, object>();
        #endregion

        #region IAIProvider Implementation
        public void Initialize(ProviderConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            ApiKey = config.ApiKey;
            BaseUrl = config.BaseUrl;
            CurrentModel = config.Model;
            IsInitialized = true;
            
            Debug.Log($"[AISDK] Initialized Cohere provider");
        }

        public void SetApiKey(string apiKey)
        {
            ApiKey = apiKey;
        }

        public void SetBaseUrl(string baseUrl)
        {
            BaseUrl = baseUrl;
        }



        public List<ModelType> GetAvailableModels()
        {
            return new List<ModelType>
            {
                ModelType.Custom // Cohere uses custom model names
            };
        }

        public bool IsModelSupported(ModelType modelType)
        {
            return GetAvailableModels().Contains(modelType);
        }

        public string GetModelName(ModelType modelType)
        {
            // Cohere uses custom model names, so we return a placeholder
            return "command";
        }

        public IEnumerator ChatAsync(ChatRequest request, Action<AIResponse> onComplete)
        {
            // Placeholder implementation
            var response = new AIResponse(true, "Cohere response placeholder", "", AgentType.Assistant)
            {
                ModelType = request.ModelType,
                TokensUsed = 10
            };
            
            onComplete?.Invoke(response);
            yield break;
        }

        public IEnumerator StreamAsync(ChatRequest request, Action<StreamingResponse> onChunk)
        {
            // Placeholder implementation
            var streamingResponse = new StreamingResponse(false, "Cohere streaming placeholder", "", AgentType.Assistant);
            onChunk?.Invoke(streamingResponse);
            yield break;
        }

        public IEnumerator TestConnection(Action<bool, string> onComplete)
        {
            // Placeholder implementation
            bool isConnected = !string.IsNullOrEmpty(ApiKey);
            onComplete?.Invoke(isConnected, isConnected ? "Connection successful" : "No API key provided");
            yield break;
        }

        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>(_statistics);
        }
        #endregion
    }
}

