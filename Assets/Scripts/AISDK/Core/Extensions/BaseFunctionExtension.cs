using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;
using Newtonsoft.Json;

namespace UniBuddhi.Core.Extensions
{
    /// <summary>
    /// Base class for extensions that can be called as functions by AI agents
    /// </summary>
    public abstract class BaseFunctionExtension : BaseExtension, IFunctionExtension
    {
        #region Function Extension Properties
        protected List<FunctionDefinition> _functionDefinitions = new List<FunctionDefinition>();
        protected Dictionary<string, string> _functionExamples = new Dictionary<string, string>();
        #endregion

        #region Unity Lifecycle
        protected override void Start()
        {
            base.Start();
            InitializeFunctions();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Initialize function definitions - must be implemented by derived classes
        /// </summary>
        protected abstract void InitializeFunctions();
        
        /// <summary>
        /// Execute the actual function logic - must be implemented by derived classes
        /// </summary>
        protected abstract IEnumerator ExecuteFunctionInternal(string functionName, Dictionary<string, object> arguments, Action<FunctionResult> onComplete);
        #endregion

        #region IFunctionExtension Implementation
        public virtual List<FunctionDefinition> GetFunctionDefinitions()
        {
            return new List<FunctionDefinition>(_functionDefinitions);
        }

        public virtual IEnumerator ExecuteFunction(FunctionCall functionCall, Action<FunctionResult> onComplete)
        {
            onComplete = onComplete ?? (_ => { });

            if (!IsEnabled || !_isInitialized)
            {
                onComplete(new FunctionResult(functionCall.Name, Name, false, "", "Extension not enabled or initialized"));
                yield break;
            }

            if (!CanHandleFunction(functionCall.Name))
            {
                onComplete(new FunctionResult(functionCall.Name, Name, false, "", $"Function {functionCall.Name} not supported"));
                yield break;
            }

            // Parse arguments from JSON
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            try
            {
                if (!string.IsNullOrEmpty(functionCall.Arguments))
                {
                    arguments = JsonConvert.DeserializeObject<Dictionary<string, object>>(functionCall.Arguments);
                }
                functionCall.ParsedArguments = arguments;
            }
            catch (Exception ex)
            {
                LogError($"Failed to parse function arguments: {ex.Message}");
                onComplete(new FunctionResult(functionCall.Name, Name, false, "", $"Invalid arguments: {ex.Message}"));
                yield break;
            }

            // Validate arguments
            if (!ValidateFunctionArguments(functionCall.Name, arguments))
            {
                onComplete(new FunctionResult(functionCall.Name, Name, false, "", "Invalid function arguments"));
                yield break;
            }

            // Execute the function
            FunctionResult result = null;
            bool executionComplete = false;

            StartCoroutine(ExecuteFunctionInternal(functionCall.Name, arguments, (r) =>
            {
                result = r;
                executionComplete = true;
            }));

            while (!executionComplete)
            {
                yield return null;
            }

            // Update statistics
            if (result != null)
            {
                _statistics[$"function_calls_{functionCall.Name}"] = (int)(_statistics.ContainsKey($"function_calls_{functionCall.Name}") ? _statistics[$"function_calls_{functionCall.Name}"] : 0) + 1;
                _statistics["total_function_calls"] = (int)(_statistics.ContainsKey("total_function_calls") ? _statistics["total_function_calls"] : 0) + 1;
                
                if (result.Success)
                {
                    LogInfo($"Function {functionCall.Name} executed successfully");
                }
                else
                {
                    LogError($"Function {functionCall.Name} failed: {result.Error}");
                }
            }

            onComplete(result ?? new FunctionResult(functionCall.Name, Name, false, "", "No result returned"));
        }

        public virtual bool CanHandleFunction(string functionName)
        {
            return _functionDefinitions.Exists(f => f.Name == functionName);
        }

        public virtual Dictionary<string, string> GetFunctionExamples()
        {
            return new Dictionary<string, string>(_functionExamples);
        }

        public virtual bool ValidateFunctionArguments(string functionName, Dictionary<string, object> arguments)
        {
            var functionDef = _functionDefinitions.Find(f => f.Name == functionName);
            if (functionDef == null) return false;

            // Check required parameters
            foreach (var required in functionDef.Parameters.Required)
            {
                if (!arguments.ContainsKey(required))
                {
                    LogWarning($"Missing required parameter: {required}");
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Add a function definition
        /// </summary>
        protected void AddFunction(string name, string description, FunctionParameters parameters, string example = "")
        {
            var function = new FunctionDefinition(name, description, parameters, Name);
            _functionDefinitions.Add(function);
            
            if (!string.IsNullOrEmpty(example))
            {
                _functionExamples[name] = example;
            }
            
            LogDebug($"Added function: {name}");
        }

        /// <summary>
        /// Create a simple parameter
        /// </summary>
        protected FunctionProperty CreateParameter(string type, string description, bool required = false, List<string> enumValues = null, object defaultValue = null)
        {
            return new FunctionProperty(type, description, enumValues, defaultValue);
        }

        /// <summary>
        /// Create function parameters
        /// </summary>
        protected FunctionParameters CreateParameters(Dictionary<string, FunctionProperty> properties, List<string> required = null)
        {
            var parameters = new FunctionParameters
            {
                Properties = properties,
                Required = required ?? new List<string>()
            };
            return parameters;
        }

        /// <summary>
        /// Get argument value with type conversion
        /// </summary>
        protected T GetArgument<T>(Dictionary<string, object> arguments, string key, T defaultValue = default(T))
        {
            if (!arguments.ContainsKey(key))
                return defaultValue;

            try
            {
                var value = arguments[key];
                if (value is T directValue)
                    return directValue;

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to convert argument {key}: {ex.Message}");
                return defaultValue;
            }
        }
        #endregion

        #region Base Extension Override
        public override bool ShouldRespond(string userMessage)
        {
            // Function extensions respond through function calls, not preprocessing
            return false;
        }

        protected override IEnumerator ProcessPreprocess(string userMessage, Action<string> onComplete)
        {
            // Function extensions don't do preprocessing
            onComplete?.Invoke("");
            yield break;
        }

        protected override IEnumerator ProcessPostprocess(string modelText, Action<ExtensionResult> onComplete)
        {
            // Function extensions don't do postprocessing
            onComplete?.Invoke(new ExtensionResult(Name, modelText));
            yield break;
        }
        #endregion
    }
}