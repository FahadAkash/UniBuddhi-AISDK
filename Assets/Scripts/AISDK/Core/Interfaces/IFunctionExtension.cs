using System;
using System.Collections;
using System.Collections.Generic;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Interfaces
{
    /// <summary>
    /// Interface for extensions that can be called as functions by AI agents
    /// </summary>
    public interface IFunctionExtension : IAgentExtension
    {
        /// <summary>
        /// Get function definitions that this extension provides
        /// </summary>
        List<FunctionDefinition> GetFunctionDefinitions();
        
        /// <summary>
        /// Execute a function call
        /// </summary>
        IEnumerator ExecuteFunction(FunctionCall functionCall, Action<FunctionResult> onComplete);
        
        /// <summary>
        /// Check if this extension can handle a specific function
        /// </summary>
        bool CanHandleFunction(string functionName);
        
        /// <summary>
        /// Get function usage examples
        /// </summary>
        Dictionary<string, string> GetFunctionExamples();
        
        /// <summary>
        /// Validate function arguments
        /// </summary>
        bool ValidateFunctionArguments(string functionName, Dictionary<string, object> arguments);
    }
}