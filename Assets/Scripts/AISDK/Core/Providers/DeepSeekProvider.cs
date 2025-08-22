using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Providers
{
    /// <summary>
    /// DeepSeek provider implementation
    /// </summary>
    public class DeepSeekProvider : MonoBehaviour, IAIProvider
    {
        #region Properties
        public AIProviderType Type => AIProviderType.DeepSeek;
        public string Name => "DeepSeek";
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
            
            Debug.Log($"[AISDK] Initialized DeepSeek provider");
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
                ModelType.DeepSeek_Coder,
                ModelType.DeepSeek_Chat
            };
        }

        public bool IsModelSupported(ModelType modelType)
        {
            return GetAvailableModels().Contains(modelType);
        }

        public string GetModelName(ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.DeepSeek_Coder:
                    return "deepseek-coder";
                case ModelType.DeepSeek_Chat:
                    return "deepseek-chat";
                default:
                    return "deepseek-chat";
            }
        }

        public IEnumerator ChatAsync(ChatRequest request, Action<AIResponse> onComplete)
        {
            // Placeholder implementation
            var response = new AIResponse(true, "DeepSeek response placeholder", "", AgentType.Assistant)
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
            var streamingResponse = new StreamingResponse(false, "DeepSeek streaming placeholder", "", AgentType.Assistant);
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

