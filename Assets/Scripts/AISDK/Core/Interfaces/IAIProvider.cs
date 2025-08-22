using System;
using System.Collections;
using System.Collections.Generic;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Interfaces
{
    /// <summary>
    /// Interface for AI providers (OpenAI, Gemini, DeepSeek, etc.)
    /// </summary>
    public interface IAIProvider
    {
        /// <summary>
        /// Provider type
        /// </summary>
        AIProviderType Type { get; }
        
        /// <summary>
        /// Provider name
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Whether the provider is initialized and ready
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Initialize the provider
        /// </summary>
        void Initialize(ProviderConfig config);
        
        /// <summary>
        /// Set the API key
        /// </summary>
        void SetApiKey(string apiKey);
        
        /// <summary>
        /// Set the base URL
        /// </summary>
        void SetBaseUrl(string baseUrl);
        
        /// <summary>
        /// Get available models for this provider
        /// </summary>
        List<ModelType> GetAvailableModels();
        
        /// <summary>
        /// Check if a model is supported
        /// </summary>
        bool IsModelSupported(ModelType modelType);
        
        /// <summary>
        /// Get the model name string for API calls
        /// </summary>
        string GetModelName(ModelType modelType);
        
        /// <summary>
        /// Send a chat request
        /// </summary>
        IEnumerator ChatAsync(ChatRequest request, Action<AIResponse> onComplete);
        
        /// <summary>
        /// Stream a chat request
        /// </summary>
        IEnumerator StreamAsync(ChatRequest request, Action<StreamingResponse> onChunk);
        
        /// <summary>
        /// Test the provider connection
        /// </summary>
        IEnumerator TestConnection(Action<bool, string> onComplete);
        
        /// <summary>
        /// Get provider statistics
        /// </summary>
        Dictionary<string, object> GetStatistics();
    }
}
