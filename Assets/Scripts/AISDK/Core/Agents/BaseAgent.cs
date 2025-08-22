using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Agents
{
    /// <summary>
    /// Base agent class implementing IAgent interface
    /// </summary>
    public abstract class BaseAgent : IAgent
    {
        #region Properties
        public AgentType Type { get; protected set; }
        public AgentConfig Config { get; protected set; }
        public IAIProvider Provider { get; protected set; }
        public List<Message> History { get; protected set; }
        public bool IsReady { get; protected set; }

        protected List<Message> _persistentContext = new List<Message>();
        protected Dictionary<string, object> _statistics = new Dictionary<string, object>();
        #endregion

        #region Constructor
        protected BaseAgent(AgentType type)
        {
            Type = type;
            History = new List<Message>();
            IsReady = false;
        }
        #endregion

        #region IAgent Implementation
        public virtual void Initialize(AgentConfig config, IAIProvider provider)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            
            if (!Provider.IsInitialized)
            {
                throw new InvalidOperationException("Provider must be initialized before creating agent");
            }

            // Add system prompt to history if provided
            if (!string.IsNullOrEmpty(Config.SystemPrompt))
            {
                History.Add(new Message("system", Config.SystemPrompt));
            }

            IsReady = true;
            _statistics["initialized"] = DateTime.Now;
            _statistics["total_messages"] = 0;
            _statistics["total_tokens"] = 0;
        }

        public virtual void SetSystemPrompt(string systemPrompt)
        {
            Config.SystemPrompt = systemPrompt ?? string.Empty;
            
            // Update system message in history
            History.RemoveAll(m => m.Role == "system");
            if (!string.IsNullOrEmpty(Config.SystemPrompt))
            {
                History.Insert(0, new Message("system", Config.SystemPrompt));
            }
        }

        public virtual void SetContext(string context)
        {
            _persistentContext.Clear();
            if (!string.IsNullOrEmpty(context))
            {
                _persistentContext.Add(new Message("user", context));
            }
        }

        public virtual void SetContext(params Message[] messages)
        {
            _persistentContext.Clear();
            if (messages != null)
            {
                foreach (var message in messages)
                {
                    if (message != null)
                    {
                        _persistentContext.Add(message);
                    }
                }
            }
        }

        public virtual void ClearContext()
        {
            _persistentContext.Clear();
        }

        public virtual void AddMessage(string role, string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                History.Add(new Message(role, content));
                _statistics["total_messages"] = (int)_statistics["total_messages"] + 1;
            }
        }

        public virtual void ClearHistory()
        {
            History.Clear();
            if (!string.IsNullOrEmpty(Config.SystemPrompt))
            {
                History.Add(new Message("system", Config.SystemPrompt));
            }
        }

        public virtual IEnumerator ChatAsync(string message, Action<AIResponse> onComplete)
        {
            if (!IsReady)
            {
                onComplete?.Invoke(new AIResponse(false, "", "Agent not ready", Type));
                yield break;
            }

            var messages = new[] { new Message("user", message) };
            yield return ChatAsync(messages, onComplete);
        }

        public virtual IEnumerator ChatAsync(Message[] messages, Action<AIResponse> onComplete)
        {
            if (!IsReady)
            {
                onComplete?.Invoke(new AIResponse(false, "", "Agent not ready", Type));
                yield break;
            }

            // Add messages to history
            foreach (var message in messages)
            {
                if (message != null)
                {
                    History.Add(message);
                    _statistics["total_messages"] = (int)_statistics["total_messages"] + 1;
                }
            }

            // Create chat request
            var request = CreateChatRequest(messages);
            
            // Send request to provider
            AIResponse response = null;
            bool requestComplete = false;
            bool requestError = false;
            string errorMessage = "";

            Provider.ChatAsync(request, (result) => 
            {
                response = result;
                requestComplete = true;
            });

            while (!requestComplete)
            {
                yield return null;
            }

            if (response == null)
            {
                errorMessage = "No response received from provider";
                requestError = true;
            }
            else if (!response.Success)
            {
                errorMessage = response.Error ?? "Unknown error from provider";
                requestError = true;
            }

            if (requestError)
            {
                Debug.LogError($"[AISDK] Agent chat error: {errorMessage}");
                onComplete?.Invoke(new AIResponse(false, "", errorMessage, Type));
                yield break;
            }

            if (response != null && response.Success)
            {
                // Add response to history
                History.Add(new Message("assistant", response.Content));
                _statistics["total_messages"] = (int)_statistics["total_messages"] + 1;
                _statistics["total_tokens"] = (long)_statistics["total_messages"] + response.TokensUsed;
            }

            onComplete?.Invoke(response);
        }

        public virtual IEnumerator StreamAsync(string message, Action<string, bool> onChunk)
        {
            if (!IsReady)
            {
                onChunk?.Invoke("Agent not ready", true);
                yield break;
            }

            var messages = new[] { new Message("user", message) };
            yield return StreamAsync(messages, onChunk);
        }

        public virtual IEnumerator StreamAsync(Message[] messages, Action<string, bool> onChunk)
        {
            if (!IsReady)
            {
                onChunk?.Invoke("Agent not ready", true);
                yield break;
            }

            // Add messages to history
            foreach (var message in messages)
            {
                if (message != null)
                {
                    History.Add(message);
                    _statistics["total_messages"] = (int)_statistics["total_messages"] + 1;
                }
            }

            // Create chat request
            var request = CreateChatRequest(messages);
            request.Stream = true;

            string fullResponse = "";

            // Stream response from provider
            bool streamComplete = false;
            bool streamError = false;
            string errorMessage = "";

            Provider.StreamAsync(request, (streamResponse) =>
            {
                if (!string.IsNullOrEmpty(streamResponse.Content))
                {
                    fullResponse += streamResponse.Content;
                    onChunk?.Invoke(streamResponse.Content, false);
                }

                if (streamResponse.IsComplete)
                {
                    onChunk?.Invoke("", true);
                    streamComplete = true;
                }
            });

            while (!streamComplete)
            {
                yield return null;
            }

            if (streamError)
            {
                Debug.LogError($"[AISDK] Agent stream error: {errorMessage}");
                onChunk?.Invoke($"Error: {errorMessage}", true);
                yield break;
            }

            // Add complete response to history
            if (!string.IsNullOrEmpty(fullResponse))
            {
                History.Add(new Message("assistant", fullResponse));
                _statistics["total_messages"] = (int)_statistics["total_messages"] + 1;
            }
        }

        public virtual Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>(_statistics)
            {
                ["agent_type"] = Type.ToString(),
                ["history_count"] = History.Count,
                ["context_count"] = _persistentContext.Count,
                ["is_ready"] = IsReady
            };
            return stats;
        }
        #endregion

        #region Protected Methods
        protected virtual ChatRequest CreateChatRequest(Message[] messages)
        {
            var request = new ChatRequest
            {
                Model = Provider.GetModelName(Config.Model),
                Temperature = Config.Temperature,
                MaxTokens = Config.MaxTokens,
                SystemPrompt = Config.SystemPrompt,
                Stream = false
            };

            // Add persistent context first
            foreach (var ctx in _persistentContext)
            {
                if (ctx.Role != "system") // Skip system messages in context
                {
                    request.Messages.Add(ctx);
                }
            }

            // Add conversation messages
            foreach (var msg in messages)
            {
                if (msg != null)
                {
                    request.Messages.Add(msg);
                }
            }

            return request;
        }

        protected virtual void LogDebug(string message)
        {
            Debug.Log($"[AISDK] [{Type}] {message}");
        }

        protected virtual void LogWarning(string message)
        {
            Debug.LogWarning($"[AISDK] [{Type}] {message}");
        }

        protected virtual void LogError(string message)
        {
            Debug.LogError($"[AISDK] [{Type}] {message}");
        }
        #endregion
    }
}
