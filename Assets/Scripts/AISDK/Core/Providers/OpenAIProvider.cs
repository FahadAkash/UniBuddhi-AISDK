using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Providers
{
    /// <summary>
    /// OpenAI provider implementation
    /// </summary>
    public class OpenAIProvider : MonoBehaviour, IAIProvider
    {
        #region Properties
        public AIProviderType Type => AIProviderType.OpenAI;
        public string Name => "OpenAI";
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
            
            Debug.Log($"[AISDK] Initialized OpenAI provider");
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
                ModelType.GPT_4,
                ModelType.GPT_4_Turbo,
                ModelType.GPT_3_5_Turbo
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
                case ModelType.GPT_4:
                    return "gpt-4";
                case ModelType.GPT_4_Turbo:
                    return "gpt-4-turbo-preview";
                case ModelType.GPT_3_5_Turbo:
                    return "gpt-3.5-turbo";
                default:
                    return "gpt-3.5-turbo";
            }
        }

        public IEnumerator ChatAsync(ChatRequest request, Action<AIResponse> onComplete)
        {
            // Placeholder implementation
            var response = new AIResponse(true, "OpenAI response placeholder", "", AgentType.Assistant)
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
            var streamingResponse = new StreamingResponse(false, "OpenAI streaming placeholder", "", AgentType.Assistant);
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

