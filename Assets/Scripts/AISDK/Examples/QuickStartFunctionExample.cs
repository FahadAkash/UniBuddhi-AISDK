using System.Collections;
using UnityEngine;
using UniBuddhi.Core;
using UniBuddhi.Core.Models;
using UniBuddhi.Core.Agents;
using UniBuddhi.Core.Extensions;

namespace UniBuddhi.Examples
{
    /// <summary>
    /// Quick start example for enhanced agents with function calling capabilities
    /// This demonstrates how to create agents with personalities and use extensions as functions
    /// </summary>
    public class QuickStartFunctionExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string apiKey = "your-api-key-here";
        [SerializeField] private string testMessage = "What's 15 * 24 + the square root of 144?";
        
        private AISDKCore aiSDK;
        private EnhancedAgent myAgent;
        
        void Start()
        {
            // Step 1: Get AI SDK
            aiSDK = AISDKCore.Instance;
            
            // Step 2: Configure API
            aiSDK.SetApiKey(apiKey);
            
            // Step 3: Add function extensions to the scene
            AddFunctionExtensions();
            
            // Step 4: Create an agent with custom personality
            CreateMathTutorAgent();
            
            // Step 5: Test the agent
            if (!string.IsNullOrEmpty(testMessage))
            {
                Invoke(nameof(TestAgent), 2f); // Wait a bit for initialization
            }
        }
        
        void AddFunctionExtensions()
        {
            // Add Calculator extension
            var calculator = gameObject.AddComponent<CalculatorExtension>();
            aiSDK.AddFunctionExtension(calculator);
            
            // Add Knowledge Search extension
            var knowledge = gameObject.AddComponent<KnowledgeSearchExtension>();
            aiSDK.AddFunctionExtension(knowledge);
            
            Debug.Log("Function extensions added");
        }
        
        void CreateMathTutorAgent()
        {
            // Create a custom personality for a math tutor
            var mathTutorPersonality = aiSDK.CreatePersonality(
                "Math Tutor",
                "You are a friendly and patient math tutor. When students ask math questions, " +
                "use your calculator functions to solve problems step-by-step. " +
                "Always explain your reasoning and show your work. " +
                "If you need to look up mathematical concepts, use your knowledge search functions.",
                new System.Collections.Generic.Dictionary<string, float> { 
                    { "patient", 0.9f }, 
                    { "educational", 0.8f }, 
                    { "step-by-step", 0.9f }, 
                    { "encouraging", 0.7f } 
                },
                new System.Collections.Generic.List<string> { "calculations", "mathematical_explanations", "problem_solving" }
            );
            
            // Create enhanced agent with function extensions
            var functionExtensions = new string[] { "Calculator", "KnowledgeSearch" };
            
            myAgent = aiSDK.CreateEnhancedAgent(
                "MathTutorBot", 
                AgentType.Technical, 
                mathTutorPersonality, 
                functionExtensions
            );
            
            if (myAgent != null)
            {
                Debug.Log($"Math Tutor Agent created with {myAgent.AvailableFunctions.Count} functions:");
                foreach (var func in myAgent.AvailableFunctions)
                {
                    Debug.Log($"  - {func.Name}: {func.Description}");
                }
            }
            else
            {
                Debug.LogError("Failed to create Math Tutor Agent");
            }
        }
        
        void TestAgent()
        {
            if (myAgent == null)
            {
                Debug.LogError("Agent not created");
                return;
            }
            
            Debug.Log($"Sending test message: {testMessage}");
            
            // Subscribe to response
            AISDKCore.OnAIResponse += OnAgentResponse;
            AISDKCore.OnError += OnAgentError;
            
            // Send message to enhanced agent
            aiSDK.SendMessageToEnhancedAgent("MathTutorBot", testMessage, false);
        }
        
        void OnAgentResponse(string response)
        {
            Debug.Log($"Agent Response:\n{response}");
            
            // Unsubscribe
            AISDKCore.OnAIResponse -= OnAgentResponse;
            AISDKCore.OnError -= OnAgentError;
        }
        
        void OnAgentError(string error)
        {
            Debug.LogError($"Agent Error: {error}");
            
            // Unsubscribe
            AISDKCore.OnAIResponse -= OnAgentResponse;
            AISDKCore.OnError -= OnAgentError;
        }
        
        #region Public Test Methods
        [ContextMenu("Test Calculator Direct")]
        public void TestCalculatorDirect()
        {
            // Test calling functions directly
            aiSDK.ExecuteFunction("multiply", "{\"a\": 15, \"b\": 24}", (result) =>
            {
                Debug.Log($"Calculator Result: {result.Result}");
                
                if (result.Success)
                {
                    // Chain another calculation
                    aiSDK.ExecuteFunction("sqrt", "{\"number\": 144}", (sqrtResult) =>
                    {
                        Debug.Log($"Square Root Result: {sqrtResult.Result}");
                        
                        if (sqrtResult.Success)
                        {
                            // Final addition
                            var sum = float.Parse(result.Result) + float.Parse(sqrtResult.Result);
                            Debug.Log($"Final Answer: {sum}");
                        }
                    });
                }
            });
        }
        
        [ContextMenu("Ask About Gravity")]
        public void AskAboutGravity()
        {
            if (myAgent != null)
            {
                AISDKCore.OnAIResponse += (response) =>
                {
                    Debug.Log($"Gravity Explanation:\n{response}");
                    AISDKCore.OnAIResponse -= OnAgentResponse; // Unsubscribe
                };
                
                aiSDK.SendMessageToEnhancedAgent("MathTutorBot", "Can you explain gravity and calculate the gravitational force between two 10kg objects 1 meter apart?", false);
            }
        }
        
        [ContextMenu("Show Available Functions")]
        public void ShowAvailableFunctions()
        {
            if (myAgent != null)
            {
                Debug.Log($"Available Functions for {myAgent.Type} Agent:");
                foreach (var func in myAgent.AvailableFunctions)
                {
                    Debug.Log($"  📊 {func.Name}: {func.Description}");
                    
                    // Show parameters
                    if (func.Parameters.Properties.Count > 0)
                    {
                        Debug.Log($"     Parameters:");
                        foreach (var param in func.Parameters.Properties)
                        {
                            var required = func.Parameters.Required.Contains(param.Key) ? "[Required]" : "[Optional]";
                            Debug.Log($"       - {param.Key} ({param.Value.Type}) {required}: {param.Value.Description}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("No agent created yet");
            }
        }
        
        [ContextMenu("Show System Status")]
        public void ShowSystemStatus()
        {
            Debug.Log(aiSDK.GetEnhancedSystemStatus());
        }
        #endregion
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (aiSDK != null)
            {
                AISDKCore.OnAIResponse -= OnAgentResponse;
                AISDKCore.OnError -= OnAgentError;
            }
        }
    }
}