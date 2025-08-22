using System;
using System.Collections;
using System.Collections.Generic;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Interfaces
{
    /// <summary>
    /// Interface for agent extensions (tools, web search, filters, etc.)
    /// </summary>
    public interface IAgentExtension
    {
        /// <summary>
        /// Extension name
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Extension version
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// Extension description
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Whether the extension is enabled
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Extension priority (lower numbers = higher priority)
        /// </summary>
        int Priority { get; set; }
        
        /// <summary>
        /// Extension configuration
        /// </summary>
        ExtensionConfig Config { get; }
        
        /// <summary>
        /// Initialize the extension
        /// </summary>
        void Initialize(ExtensionConfig config);
        
        /// <summary>
        /// Preprocess user message and return additional context
        /// </summary>
        IEnumerator Preprocess(string userMessage, Action<ExtensionContext> onContextReady);
        
        /// <summary>
        /// Postprocess AI response and optionally modify it
        /// </summary>
        IEnumerator Postprocess(string modelText, Action<ExtensionResult> onResultReady);
        
        /// <summary>
        /// Check if this extension should respond to the given message
        /// </summary>
        bool ShouldRespond(string userMessage);
        
        /// <summary>
        /// Get extension metadata
        /// </summary>
        Dictionary<string, object> GetMetadata();
        
        /// <summary>
        /// Get extension statistics
        /// </summary>
        Dictionary<string, object> GetStatistics();
        
        /// <summary>
        /// Validate extension configuration
        /// </summary>
        bool ValidateConfig();
        
        /// <summary>
        /// Test extension functionality
        /// </summary>
        IEnumerator TestExtension(Action<bool, string> onComplete);
    }
}
