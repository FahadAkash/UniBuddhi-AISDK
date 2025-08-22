using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;
using UniBuddhi.Core.Extensions;
using Newtonsoft.Json;

namespace UniBuddhi.Core
{
    /// <summary>
    /// Manages function calling between AI agents and extensions
    /// </summary>
    public class FunctionCallManager : MonoBehaviour
    {
        #region Events
        public static event Action<FunctionCall> OnFunctionCalled;
        public static event Action<FunctionResult> OnFunctionCompleted;
        public static event Action<string, string> OnFunctionRegistered;
        public static event Action<string, string> OnFunctionUnregistered;
        #endregion

        #region Properties
        public Dictionary<string, IFunctionExtension> RegisteredExtensions { get; private set; }
        public List<FunctionDefinition> AvailableFunctions { get; private set; }
        public bool IsInitialized { get; private set; }
        #endregion

        #region Inspector Settings
        [Header("Function Call Manager Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private int maxConcurrentCalls = 5;
        [SerializeField] private float functionTimeoutSeconds = 30f;
        [SerializeField] private bool enableFunctionValidation = true;
        #endregion

        #region Private Fields
        private Dictionary<string, List<FunctionDefinition>> _extensionFunctions = new Dictionary<string, List<FunctionDefinition>>();
        private Dictionary<string, FunctionExecutionContext> _activeCalls = new Dictionary<string, FunctionExecutionContext>();
        private Dictionary<string, object> _statistics = new Dictionary<string, object>();
        private int _functionCallCounter = 0;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            LogInfo("Function Call Manager started");
        }

        private void Update()
        {
            // Check for timed out function calls
            CheckForTimeouts();
        }
        #endregion

        #region Initialization
        public void Initialize()
        {
            RegisteredExtensions = new Dictionary<string, IFunctionExtension>();
            AvailableFunctions = new List<FunctionDefinition>();
            
            InitializeStatistics();
            IsInitialized = true;
            
            LogInfo("Function Call Manager initialized");
        }

        private void InitializeStatistics()
        {
            _statistics["total_functions_registered"] = 0;
            _statistics["total_calls_executed"] = 0;
            _statistics["successful_calls"] = 0;
            _statistics["failed_calls"] = 0;
            _statistics["average_execution_time"] = 0.0;
            _statistics["active_extensions"] = 0;
        }
        #endregion

        #region Extension Registration
        public void RegisterExtension(IFunctionExtension extension)
        {
            if (extension == null)
            {
                LogWarning("Cannot register null extension");
                return;
            }

            var extensionName = extension.Name;
            if (RegisteredExtensions.ContainsKey(extensionName))
            {
                LogWarning($"Extension {extensionName} is already registered");
                return;
            }

            // Register the extension
            RegisteredExtensions[extensionName] = extension;
            
            // Get and register functions
            var functions = extension.GetFunctionDefinitions();
            _extensionFunctions[extensionName] = new List<FunctionDefinition>(functions);
            
            foreach (var function in functions)
            {
                function.ExtensionName = extensionName;
                AvailableFunctions.Add(function);
            }

            // Update statistics
            _statistics["total_functions_registered"] = (int)_statistics["total_functions_registered"] + functions.Count;
            _statistics["active_extensions"] = RegisteredExtensions.Count;

            OnFunctionRegistered?.Invoke(extensionName, $"{functions.Count} functions registered");
            LogInfo($"Registered extension: {extensionName} with {functions.Count} functions");
        }

        public void UnregisterExtension(string extensionName)
        {
            if (!RegisteredExtensions.ContainsKey(extensionName))
            {
                LogWarning($"Extension {extensionName} is not registered");
                return;
            }

            // Remove functions
            if (_extensionFunctions.ContainsKey(extensionName))
            {
                var functions = _extensionFunctions[extensionName];
                foreach (var function in functions)
                {
                    AvailableFunctions.RemoveAll(f => f.Name == function.Name && f.ExtensionName == extensionName);
                }
                _extensionFunctions.Remove(extensionName);
            }

            // Remove extension
            RegisteredExtensions.Remove(extensionName);
            
            // Update statistics
            _statistics["active_extensions"] = RegisteredExtensions.Count;

            OnFunctionUnregistered?.Invoke(extensionName, "Extension unregistered");
            LogInfo($"Unregistered extension: {extensionName}");
        }

        public void UnregisterExtension(IFunctionExtension extension)
        {
            if (extension != null)
            {
                UnregisterExtension(extension.Name);
            }
        }
        #endregion

        #region Function Execution
        public IEnumerator ExecuteFunctionAsync(FunctionCall functionCall, Action<FunctionResult> onComplete)
        {
            var callId = GenerateCallId();
            var startTime = Time.time;

            // Validate function call
            if (enableFunctionValidation && !ValidateFunctionCall(functionCall))
            {
                var error = $"Invalid function call: {functionCall.Name}";
                LogError(error);
                onComplete?.Invoke(new FunctionResult(functionCall.Name, "", false, "", error));
                yield break;
            }

            // Check concurrent call limit
            if (_activeCalls.Count >= maxConcurrentCalls)
            {
                var error = $"Maximum concurrent calls limit reached ({maxConcurrentCalls})";
                LogWarning(error);
                onComplete?.Invoke(new FunctionResult(functionCall.Name, "", false, "", error));
                yield break;
            }

            // Find the extension that handles this function
            var extension = FindExtensionForFunction(functionCall.Name);
            if (extension == null)
            {
                var error = $"No extension found for function: {functionCall.Name}";
                LogError(error);
                onComplete?.Invoke(new FunctionResult(functionCall.Name, "", false, "", error));
                yield break;
            }

            // Execute function with exception handling
            yield return StartCoroutine(ExecuteFunctionInternal(functionCall, extension, callId, startTime, onComplete));
        }

        private IEnumerator ExecuteFunctionInternal(FunctionCall functionCall, IFunctionExtension extension, string callId, float startTime, Action<FunctionResult> onComplete)
        {
            FunctionResult result = null;
            
            // Initialize execution context
            var context = new FunctionExecutionContext
            {
                CallId = callId,
                FunctionCall = functionCall,
                Extension = extension,
                StartTime = startTime,
                IsComplete = false
            };
            
            try
            {
                _activeCalls[callId] = context;
                LogDebug($"Executing function: {functionCall.Name} (Call ID: {callId})");
                OnFunctionCalled?.Invoke(functionCall);
            }
            catch (Exception ex)
            {
                LogError($"Exception during function setup: {ex.Message}");
                result = new FunctionResult(functionCall.Name, "", false, "", ex.Message);
                onComplete?.Invoke(result);
                yield break;
            }

            // Execute the function (outside try-catch to allow yield)
            bool executionComplete = false;
            
            extension.ExecuteFunction(functionCall, (r) =>
            {
                result = r;
                executionComplete = true;
                context.IsComplete = true;
                context.Result = r;
            });

            // Wait for completion or timeout
            var timeout = Time.time + functionTimeoutSeconds;
            while (!executionComplete && Time.time < timeout)
            {
                yield return null;
            }

            // Handle timeout
            if (!executionComplete)
            {
                var error = $"Function call timed out after {functionTimeoutSeconds} seconds";
                LogError(error);
                result = new FunctionResult(functionCall.Name, extension.Name, false, "", error);
            }

            // Post-execution cleanup and statistics (with exception handling)
            try
            {
                var executionTime = Time.time - startTime;
                UpdateExecutionStatistics(result?.Success ?? false, executionTime);
                _activeCalls.Remove(callId);
                LogDebug($"Function {functionCall.Name} completed in {executionTime:F2}s (Success: {result?.Success ?? false})");
                OnFunctionCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                LogError($"Exception during function cleanup: {ex.Message}");
                _activeCalls.Remove(callId);
                if (result == null)
                {
                    result = new FunctionResult(functionCall.Name, "", false, "", ex.Message);
                }
            }
            
            onComplete?.Invoke(result);
        }

        public IEnumerator ExecuteMultipleFunctionsAsync(List<FunctionCall> functionCalls, Action<List<FunctionResult>> onComplete)
        {
            var results = new List<FunctionResult>();
            var completedCount = 0;
            var totalCalls = functionCalls.Count;

            LogInfo($"Executing {totalCalls} functions simultaneously");

            // Start all function calls
            foreach (var functionCall in functionCalls)
            {
                StartCoroutine(ExecuteFunctionAsync(functionCall, (result) =>
                {
                    results.Add(result);
                    completedCount++;
                }));
            }

            // Wait for all to complete
            while (completedCount < totalCalls)
            {
                yield return null;
            }

            LogInfo($"Completed {totalCalls} function calls");
            onComplete?.Invoke(results);
        }
        #endregion

        #region Function Discovery
        public List<FunctionDefinition> GetFunctionsForExtension(string extensionName)
        {
            return _extensionFunctions.ContainsKey(extensionName) 
                ? new List<FunctionDefinition>(_extensionFunctions[extensionName])
                : new List<FunctionDefinition>();
        }

        public List<FunctionDefinition> GetFunctionsByCategory(string category)
        {
            return AvailableFunctions
                .Where(f => f.Description.ToLowerInvariant().Contains(category.ToLowerInvariant()))
                .ToList();
        }

        public FunctionDefinition GetFunctionDefinition(string functionName)
        {
            return AvailableFunctions.FirstOrDefault(f => f.Name == functionName);
        }

        public bool IsFunctionAvailable(string functionName)
        {
            return AvailableFunctions.Any(f => f.Name == functionName);
        }

        public List<string> GetExtensionNames()
        {
            return RegisteredExtensions.Keys.ToList();
        }
        #endregion

        #region Validation
        private bool ValidateFunctionCall(FunctionCall functionCall)
        {
            if (string.IsNullOrEmpty(functionCall.Name))
            {
                LogWarning("Function call has empty name");
                return false;
            }

            var functionDef = GetFunctionDefinition(functionCall.Name);
            if (functionDef == null)
            {
                LogWarning($"Function {functionCall.Name} not found");
                return false;
            }

            // Validate arguments if provided
            if (!string.IsNullOrEmpty(functionCall.Arguments))
            {
                try
                {
                    var arguments = JsonConvert.DeserializeObject<Dictionary<string, object>>(functionCall.Arguments);
                    var extension = FindExtensionForFunction(functionCall.Name);
                    
                    if (extension != null && !extension.ValidateFunctionArguments(functionCall.Name, arguments))
                    {
                        LogWarning($"Invalid arguments for function {functionCall.Name}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Failed to parse function arguments: {ex.Message}");
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Helper Methods
        private IFunctionExtension FindExtensionForFunction(string functionName)
        {
            foreach (var kvp in _extensionFunctions)
            {
                if (kvp.Value.Any(f => f.Name == functionName))
                {
                    return RegisteredExtensions[kvp.Key];
                }
            }
            return null;
        }

        private string GenerateCallId()
        {
            return $"call_{++_functionCallCounter}_{DateTime.Now:HHmmss}";
        }

        private void UpdateExecutionStatistics(bool success, float executionTime)
        {
            _statistics["total_calls_executed"] = (int)_statistics["total_calls_executed"] + 1;
            
            if (success)
            {
                _statistics["successful_calls"] = (int)_statistics["successful_calls"] + 1;
            }
            else
            {
                _statistics["failed_calls"] = (int)_statistics["failed_calls"] + 1;
            }

            // Update average execution time
            var totalCalls = (int)_statistics["total_calls_executed"];
            var currentAverage = (double)_statistics["average_execution_time"];
            var newAverage = (currentAverage * (totalCalls - 1) + executionTime) / totalCalls;
            _statistics["average_execution_time"] = newAverage;
        }

        private void CheckForTimeouts()
        {
            var currentTime = Time.time;
            var timeoutCalls = _activeCalls.Values
                .Where(context => currentTime - context.StartTime > functionTimeoutSeconds)
                .ToList();

            foreach (var context in timeoutCalls)
            {
                LogWarning($"Function call {context.FunctionCall.Name} timed out (Call ID: {context.CallId})");
                _activeCalls.Remove(context.CallId);
            }
        }
        #endregion

        #region Statistics and Monitoring
        public Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>(_statistics)
            {
                ["active_calls"] = _activeCalls.Count,
                ["registered_extensions"] = RegisteredExtensions.Count,
                ["available_functions"] = AvailableFunctions.Count,
                ["success_rate"] = CalculateSuccessRate()
            };
            return stats;
        }

        private double CalculateSuccessRate()
        {
            var totalCalls = (int)_statistics["total_calls_executed"];
            if (totalCalls == 0) return 0.0;
            
            var successfulCalls = (int)_statistics["successful_calls"];
            return (double)successfulCalls / totalCalls * 100.0;
        }

        public string GetSystemStatus()
        {
            var stats = GetStatistics();
            return $"Function Call Manager Status:\n" +
                   $"- Extensions: {stats["registered_extensions"]}\n" +
                   $"- Functions: {stats["available_functions"]}\n" +
                   $"- Total Calls: {stats["total_calls_executed"]}\n" +
                   $"- Success Rate: {stats["success_rate"]:F1}%\n" +
                   $"- Active Calls: {stats["active_calls"]}\n" +
                   $"- Avg Execution Time: {stats["average_execution_time"]:F2}s";
        }
        #endregion

        #region Logging
        private void LogInfo(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[AISDK] [FunctionCallManager] {message}");
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[AISDK] [FunctionCallManager] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[AISDK] [FunctionCallManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[AISDK] [FunctionCallManager] {message}");
        }
        #endregion

        #region Function Execution Context
        private class FunctionExecutionContext
        {
            public string CallId { get; set; }
            public FunctionCall FunctionCall { get; set; }
            public IFunctionExtension Extension { get; set; }
            public float StartTime { get; set; }
            public bool IsComplete { get; set; }
            public FunctionResult Result { get; set; }
        }
        #endregion
    }
}