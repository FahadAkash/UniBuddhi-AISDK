using System;
using System.Collections;
using System.Collections.Generic;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Interfaces
{
    /// <summary>
    /// Interface for AI agents
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        /// Agent type
        /// </summary>
        AgentType Type { get; }
        
        /// <summary>
        /// Agent configuration
        /// </summary>
        AgentConfig Config { get; }
        
        /// <summary>
        /// Associated AI provider
        /// </summary>
        IAIProvider Provider { get; }
        
        /// <summary>
        /// Conversation history
        /// </summary>
        List<Message> History { get; }
        
        /// <summary>
        /// Whether the agent is ready
        /// </summary>
        bool IsReady { get; }
        
        /// <summary>
        /// Initialize the agent
        /// </summary>
        void Initialize(AgentConfig config, IAIProvider provider);
        
        /// <summary>
        /// Set the system prompt
        /// </summary>
        void SetSystemPrompt(string systemPrompt);
        
        /// <summary>
        /// Set context for the next request
        /// </summary>
        void SetContext(string context);
        
        /// <summary>
        /// Set context messages for the next request
        /// </summary>
        void SetContext(params Message[] messages);
        
        /// <summary>
        /// Clear the current context
        /// </summary>
        void ClearContext();
        
        /// <summary>
        /// Add a message to history
        /// </summary>
        void AddMessage(string role, string content);
        
        /// <summary>
        /// Clear conversation history
        /// </summary>
        void ClearHistory();
        
        /// <summary>
        /// Send a chat message
        /// </summary>
        IEnumerator ChatAsync(string message, Action<AIResponse> onComplete);
        
        /// <summary>
        /// Send a chat message with custom messages
        /// </summary>
        IEnumerator ChatAsync(Message[] messages, Action<AIResponse> onComplete);
        
        /// <summary>
        /// Stream a chat message
        /// </summary>
        IEnumerator StreamAsync(string message, Action<string, bool> onChunk);
        
        /// <summary>
        /// Stream a chat message with custom messages
        /// </summary>
        IEnumerator StreamAsync(Message[] messages, Action<string, bool> onChunk);
        
        /// <summary>
        /// Get agent statistics
        /// </summary>
        Dictionary<string, object> GetStatistics();
    }
}
