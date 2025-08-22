using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;
using UniBuddhi.Core.Extensions;
using Newtonsoft.Json;

namespace UniBuddhi.Core.Agents
{
    /// <summary>
    /// Enhanced agent with function calling capabilities using extensions
    /// </summary>
    public class EnhancedAgent : BaseAgent
    {
        #region Enhanced Properties
        public List<IFunctionExtension> FunctionExtensions { get; private set; }
        public List<FunctionDefinition> AvailableFunctions { get; private set; }
        public AgentPersonality Personality { get; private set; }
        public bool FunctionCallingEnabled { get; set; } = true;
        #endregion

        #region Private Fields
        private Dictionary<string, IFunctionExtension> _functionMap = new Dictionary<string, IFunctionExtension>();
        private List<FunctionCall> _pendingFunctionCalls = new List<FunctionCall>();
        private int _maxFunctionCallsPerMessage = 5;
        private bool _processingFunctions = false;
        #endregion

        #region Constructor
        public EnhancedAgent(AgentType type) : base(type)
        {
            FunctionExtensions = new List<IFunctionExtension>();
            AvailableFunctions = new List<FunctionDefinition>();
        }
        #endregion

        #region Enhanced Initialization
        public void Initialize(EnhancedAgentConfig config, IAIProvider provider)
        {
            base.Initialize(config, provider);
            
            // Set personality
            if (config.Personality != null)
            {
                SetPersonality(config.Personality);
            }
            
            // Configure function calling
            FunctionCallingEnabled = config.EnableFunctionCalling;
            
            LogDebug($"Enhanced agent initialized with personality: {Personality?.Name ?? "None"}");
        }

        public void SetPersonality(AgentPersonality personality)
        {
            Personality = personality;
            
            if (personality != null)
            {
                // Update system prompt with personality
                var enhancedPrompt = BuildPersonalityEnhancedPrompt(personality);
                SetSystemPrompt(enhancedPrompt);
                
                LogDebug($"Applied personality: {personality.Name}");
            }
        }

        private string BuildPersonalityEnhancedPrompt(AgentPersonality personality)
        {
            var prompt = personality.SystemPrompt;
            
            // Add function capabilities information
            if (FunctionCallingEnabled && AvailableFunctions.Any())
            {
                prompt += "\n\nAvailable Functions:\n";
                prompt += "You have access to the following functions that you can call to help users:\n";
                
                foreach (var function in AvailableFunctions)
                {
                    prompt += $"- {function.Name}: {function.Description}\n";
                }
                
                prompt += "\nWhen you need to use these functions, call them with appropriate parameters. ";
                prompt += "Always explain what you're doing when calling functions to help the user understand your process.";
            }
            
            // Add personality traits
            if (personality.Traits.Any())
            {
                prompt += $"\n\nPersonality Traits: {string.Join(", ", personality.Traits)}";
            }
            
            return prompt;
        }
        #endregion

        #region Function Extension Management
        public void AddFunctionExtension(IFunctionExtension extension)
        {
            if (extension == null) return;
            
            if (!FunctionExtensions.Contains(extension))
            {
                FunctionExtensions.Add(extension);
                
                // Register functions
                var functions = extension.GetFunctionDefinitions();
                foreach (var function in functions)
                {
                    _functionMap[function.Name] = extension;
                    AvailableFunctions.Add(function);
                }
                
                // Update system prompt if personality is set
                if (Personality != null)
                {
                    SetPersonality(Personality);
                }
                
                LogDebug($"Added function extension: {extension.Name} with {functions.Count} functions");
            }
        }

        public void RemoveFunctionExtension(IFunctionExtension extension)
        {
            if (extension == null) return;
            
            if (FunctionExtensions.Remove(extension))
            {
                // Remove functions
                var functionsToRemove = AvailableFunctions.Where(f => f.ExtensionName == extension.Name).ToList();
                foreach (var function in functionsToRemove)
                {
                    _functionMap.Remove(function.Name);
                    AvailableFunctions.Remove(function);
                }
                
                // Update system prompt
                if (Personality != null)
                {
                    SetPersonality(Personality);
                }
                
                LogDebug($"Removed function extension: {extension.Name}");
            }
        }

        public bool HasFunction(string functionName)
        {
            return _functionMap.ContainsKey(functionName);
        }
        #endregion

        #region Enhanced Chat Methods
        public override IEnumerator ChatAsync(Message[] messages, Action<AIResponse> onComplete)
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

            // Start enhanced chat process
            yield return ProcessEnhancedChat(messages, onComplete);
        }

        private IEnumerator ProcessEnhancedChat(Message[] messages, Action<AIResponse> onComplete)
        {
            var finalResponse = "";
            var functionCallCount = 0;
            var maxIterations = _maxFunctionCallsPerMessage + 1; // Allow for one final response after functions
            var currentMessages = new List<Message>(messages);

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                // Create enhanced chat request
                var request = CreateEnhancedChatRequest(currentMessages.ToArray());
                
                // Get AI response
                AIResponse response = null;
                bool requestComplete = false;

                Provider.ChatAsync(request, (result) => 
                {
                    response = result;
                    requestComplete = true;
                });

                while (!requestComplete)
                {
                    yield return null;
                }

                if (response == null || !response.Success)
                {
                    var error = response?.Error ?? "No response received from provider";
                    LogError($"Chat error: {error}");
                    onComplete?.Invoke(new AIResponse(false, "", error, Type));
                    yield break;
                }

                // Check if response contains function calls
                var functionCalls = ExtractFunctionCalls(response.Content);
                
                if (functionCalls.Any() && FunctionCallingEnabled && functionCallCount < _maxFunctionCallsPerMessage)
                {
                    // Execute function calls
                    var functionResults = new List<FunctionResult>();
                    
                    foreach (var functionCall in functionCalls)
                    {
                        FunctionResult result = null;
                        bool functionComplete = false;
                        
                        yield return ExecuteFunctionCall(functionCall, (r) => 
                        {
                            result = r;
                            functionComplete = true;
                        });
                        
                        while (!functionComplete)
                        {
                            yield return null;
                        }
                        
                        functionResults.Add(result);
                        functionCallCount++;
                    }
                    
                    // Add function results to conversation
                    var functionResultMessage = BuildFunctionResultMessage(functionResults);
                    currentMessages.Add(new Message("system", functionResultMessage));
                    
                    LogDebug($"Executed {functionResults.Count} function calls");
                }
                else
                {
                    // No more function calls, this is the final response
                    finalResponse = response.Content;
                    
                    // Add final response to history
                    History.Add(new Message("assistant", finalResponse));
                    _statistics["total_messages"] = (int)_statistics["total_messages"] + 1;
                    _statistics["total_tokens"] = (long)_statistics["total_tokens"] + response.TokensUsed;
                    
                    break;
                }
            }

            // Return final response
            var finalAIResponse = new AIResponse(true, finalResponse, "", Type)
            {
                TokensUsed = 0, // We'd need to track this properly
                Metadata = new Dictionary<string, object>
                {
                    ["function_calls_executed"] = functionCallCount,
                    ["personality"] = Personality?.Name ?? "None"
                }
            };

            onComplete?.Invoke(finalAIResponse);
        }

        private EnhancedChatRequest CreateEnhancedChatRequest(Message[] messages)
        {
            var request = new EnhancedChatRequest
            {
                Model = Provider.GetModelName(Config.Model),
                Temperature = Config.Temperature,
                MaxTokens = Config.MaxTokens,
                SystemPrompt = Config.SystemPrompt,
                SupportsFunctionCalling = FunctionCallingEnabled
            };

            // Add available functions
            if (FunctionCallingEnabled)
            {
                request.Functions.AddRange(AvailableFunctions);
                request.EnabledExtensions.AddRange(FunctionExtensions.Select(e => e.Name));
            }

            // Add persistent context
            foreach (var ctx in _persistentContext)
            {
                if (ctx.Role != "system")
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
        #endregion

        #region Function Call Processing
        private List<FunctionCall> ExtractFunctionCalls(string responseContent)
        {
            var functionCalls = new List<FunctionCall>();
            
            try
            {
                // Look for function call patterns in the response
                // This is a simplified implementation - in reality, you'd need to parse the AI provider's specific format
                
                var lines = responseContent.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("FUNCTION_CALL:"))
                    {
                        var callData = line.Substring("FUNCTION_CALL:".Length).Trim();
                        var functionCall = JsonConvert.DeserializeObject<FunctionCall>(callData);
                        
                        if (HasFunction(functionCall.Name))
                        {
                            functionCall.ExtensionName = _functionMap[functionCall.Name].Name;
                            functionCalls.Add(functionCall);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to extract function calls: {ex.Message}");
            }
            
            return functionCalls;
        }

        private IEnumerator ExecuteFunctionCall(FunctionCall functionCall, Action<FunctionResult> onComplete)
        {
            if (!_functionMap.ContainsKey(functionCall.Name))
            {
                onComplete?.Invoke(new FunctionResult(functionCall.Name, "", false, "", "Function not found"));
                yield break;
            }

            var extension = _functionMap[functionCall.Name];
            FunctionResult result = null;
            bool executionComplete = false;

            LogDebug($"Executing function: {functionCall.Name} from extension: {extension.Name}");

            extension.ExecuteFunction(functionCall, (r) => 
            {
                result = r;
                executionComplete = true;
            });

            while (!executionComplete)
            {
                yield return null;
            }

            onComplete?.Invoke(result);
        }

        private string BuildFunctionResultMessage(List<FunctionResult> results)
        {
            var message = "Function execution results:\n";
            
            foreach (var result in results)
            {
                message += $"- {result.FunctionName}: ";
                if (result.Success)
                {
                    message += $"SUCCESS - {result.Result}\n";
                }
                else
                {
                    message += $"ERROR - {result.Error}\n";
                }
            }
            
            return message;
        }
        #endregion

        #region Enhanced Statistics
        public override Dictionary<string, object> GetStatistics()
        {
            var stats = base.GetStatistics();
            
            stats["personality"] = Personality?.Name ?? "None";
            stats["function_calling_enabled"] = FunctionCallingEnabled;
            stats["available_functions"] = AvailableFunctions.Count;
            stats["function_extensions"] = FunctionExtensions.Count;
            
            if (FunctionExtensions.Any())
            {
                stats["extension_names"] = string.Join(", ", FunctionExtensions.Select(e => e.Name));
            }
            
            return stats;
        }
        #endregion

        #region Helper Methods
        protected override void LogDebug(string message)
        {
            var personalityInfo = Personality != null ? $"[{Personality.Name}]" : "";
            Debug.Log($"[AISDK] [Enhanced{Type}]{personalityInfo} {message}");
        }

        protected override void LogWarning(string message)
        {
            var personalityInfo = Personality != null ? $"[{Personality.Name}]" : "";
            Debug.LogWarning($"[AISDK] [Enhanced{Type}]{personalityInfo} {message}");
        }

        protected override void LogError(string message)
        {
            var personalityInfo = Personality != null ? $"[{Personality.Name}]" : "";
            Debug.LogError($"[AISDK] [Enhanced{Type}]{personalityInfo} {message}");
        }
        #endregion
    }
}