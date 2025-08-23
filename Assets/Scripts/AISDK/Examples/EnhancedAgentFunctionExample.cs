using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniBuddhi.Core;
using UniBuddhi.Core.Models;
using UniBuddhi.Core.Agents;
using UniBuddhi.Core.Extensions;

namespace UniBuddhi.Examples
{
    /// <summary>
    /// Comprehensive example demonstrating enhanced agents with function calling capabilities
    /// This shows how agents can have custom personalities and use extensions as callable functions
    /// </summary>
    public class EnhancedAgentFunctionExample : MonoBehaviour
    {
        [Header("UI References")]
        public InputField messageInput;
        public Text responseText;
        public Button sendButton;
        public Button createAgentButton;
        public Dropdown agentDropdown;
        public Dropdown personalityDropdown;
        public Text systemStatusText;
        public Text availableFunctionsText;
        
        [Header("Agent Configuration")]
        public string defaultAgentId = "MyAssistant";
        public AgentType defaultAgentType = AgentType.Assistant;
        
        [Header("API Configuration")]
        [SerializeField] private string openAIApiKey = "your-openai-key-here";
        [SerializeField] private string geminiApiKey = "your-gemini-key-here";
        
        private AISDKCore aiSDK;
        private Dictionary<string, AgentPersonality> availablePersonalities;
        private List<string> createdAgents = new List<string>();
        
        void Start()
        {
            // Initialize
            InitializeSDK();
            InitializePersonalities();
            SetupUI();
            UpdateSystemStatus();
        }
        
        void InitializeSDK()
        {
            // Find or create AI SDK
            aiSDK = FindFirstObjectByType<AISDKCore>();
            
            if (aiSDK == null)
            {
                Debug.LogError("AISDKCore not found! Please add it to a GameObject in the scene.");
                return;
            }
            
            // Subscribe to events
            AISDKCore.OnAIResponse += HandleAIResponse;
            AISDKCore.OnError += HandleError;
            AISDKCore.OnAudioGenerated += HandleAudioGenerated;
            AISDKCore.OnAgentCreated += HandleAgentCreated;
            
            // Configure API keys
            ConfigureAPIKeys();
            
            // Add function extensions to the scene if they don't exist
            EnsureFunctionExtensions();
            
            Debug.Log("Enhanced Agent Function Example initialized");
        }
        
        void InitializePersonalities()
        {
            availablePersonalities = new Dictionary<string, AgentPersonality>();
            
            // Helpful Assistant
            availablePersonalities["Helpful Assistant"] = aiSDK.CreatePersonality(
                "Helpful Assistant",
                "You are a helpful AI assistant with access to various tools and functions. " +
                "When users ask questions that require calculations, weather information, or knowledge lookup, " +
                "use the appropriate functions to provide accurate and helpful responses. " +
                "Always explain what functions you're using and why.",
                new Dictionary<string, float> { 
                    { "helpful", 0.9f }, 
                    { "clear", 0.8f }, 
                    { "informative", 0.8f }, 
                    { "patient", 0.7f } 
                },
                new List<string> { "calculations", "weather_info", "knowledge_search", "unit_conversion" }
            );
            
            // Smart Researcher
            availablePersonalities["Smart Researcher"] = aiSDK.CreatePersonality(
                "Smart Researcher",
                "You are a smart research assistant who loves to find and analyze information. " +
                "You have access to knowledge databases and calculation tools. " +
                "When answering questions, you provide thorough research, cite your sources, " +
                "and perform any necessary calculations to support your findings.",
                new Dictionary<string, float> { 
                    { "analytical", 0.9f }, 
                    { "thorough", 0.8f }, 
                    { "curious", 0.7f }, 
                    { "methodical", 0.8f } 
                },
                new List<string> { "knowledge_search", "calculations", "data_analysis" }
            );
            
            // Weather Expert
            availablePersonalities["Weather Expert"] = aiSDK.CreatePersonality(
                "Weather Expert",
                "You are a weather expert with access to current weather data and forecasting tools. " +
                "You provide detailed weather information, explain weather patterns, " +
                "and can answer questions about meteorology using your weather functions.",
                new Dictionary<string, float> { 
                    { "knowledgeable", 0.9f }, 
                    { "precise", 0.8f }, 
                    { "informative", 0.8f }, 
                    { "weather-focused", 0.9f } 
                },
                new List<string> { "weather_info", "forecasting", "climate_analysis" }
            );
            
            // Math Tutor
            availablePersonalities["Math Tutor"] = aiSDK.CreatePersonality(
                "Math Tutor",
                "You are a friendly math tutor with access to calculation tools. " +
                "You help students understand mathematical concepts by solving problems step-by-step, " +
                "using your calculator functions to demonstrate solutions, and explaining the reasoning behind each step.",
                new Dictionary<string, float> { 
                    { "patient", 0.9f }, 
                    { "educational", 0.8f }, 
                    { "encouraging", 0.7f }, 
                    { "step-by-step", 0.9f } 
                },
                new List<string> { "calculations", "mathematical_analysis", "problem_solving" }
            );
        }
        
        void SetupUI()
        {
            // Setup buttons
            sendButton.onClick.AddListener(SendMessage);
            createAgentButton.onClick.AddListener(CreateNewAgent);
            
            // Setup dropdowns
            PopulatePersonalityDropdown();
            
            // Initialize response text
            responseText.text = "Enhanced AI SDK Ready! Create an agent with a personality and start chatting.\\n\\n" +
                              "Try asking questions like:\\n" +
                              "• 'What's 25 * 47 + 133?'\\n" +
                              "• 'What's the weather like in Tokyo?'\\n" +
                              "• 'Tell me about photosynthesis'\\n" +
                              "• 'Convert 5 feet to meters'";
        }
        
        void PopulatePersonalityDropdown()
        {
            personalityDropdown.ClearOptions();
            var options = new List<string>(availablePersonalities.Keys);
            personalityDropdown.AddOptions(options);
        }
        
        void ConfigureAPIKeys()
        {
            // Set API keys based on current provider
            switch (aiSDK.CurrentProvider)
            {
                case AIProviderType.OpenAI:
                    aiSDK.SetApiKey(openAIApiKey);
                    break;
                case AIProviderType.Gemini:
                    aiSDK.SetApiKey(geminiApiKey);
                    break;
            }
        }
        
        void EnsureFunctionExtensions()
        {
            // Add Calculator Extension
            var calculator = GetComponent<CalculatorExtension>();
            if (calculator == null)
            {
                calculator = gameObject.AddComponent<CalculatorExtension>();
            }
            aiSDK.AddFunctionExtension(calculator);
            
            // Add Weather Extension
            var weather = GetComponent<WeatherInfoExtension>();
            if (weather == null)
            {
                weather = gameObject.AddComponent<WeatherInfoExtension>();
            }
            aiSDK.AddFunctionExtension(weather);
            
            // Add Knowledge Search Extension
            var knowledge = GetComponent<KnowledgeSearchExtension>();
            if (knowledge == null)
            {
                knowledge = gameObject.AddComponent<KnowledgeSearchExtension>();
            }
            aiSDK.AddFunctionExtension(knowledge);
            
            Debug.Log("Function extensions ensured");
        }
        
        public void CreateNewAgent()
        {
            var personalityName = personalityDropdown.options[personalityDropdown.value].text;
            var personality = availablePersonalities[personalityName];
            
            var agentId = $"Agent_{personalityName.Replace(" ", "")}_{createdAgents.Count}";
            
            // Create enhanced agent with all function extensions
            var functionExtensions = new string[] { "Calculator", "WeatherInfo", "KnowledgeSearch" };
            var agent = aiSDK.CreateEnhancedAgent(agentId, defaultAgentType, personality, functionExtensions);
            
            if (agent != null)
            {
                createdAgents.Add(agentId);
                UpdateAgentDropdown();
                UpdateSystemStatus();
                UpdateAvailableFunctions(agent);
                
                responseText.text = $"Created agent '{agentId}' with personality '{personalityName}'!\\n\\n" +
                                  $"This agent has access to {agent.AvailableFunctions.Count} functions:\\n" +
                                  $"• Calculator functions (add, multiply, etc.)\\n" +
                                  $"• Weather information functions\\n" +
                                  $"• Knowledge search functions\\n\\n" +
                                  $"Try asking complex questions that require multiple tools!";
                
                Debug.Log($"Created enhanced agent: {agentId} with {agent.AvailableFunctions.Count} functions");
            }
        }
        
        void UpdateAgentDropdown()
        {
            agentDropdown.ClearOptions();
            agentDropdown.AddOptions(createdAgents);
        }
        
        public void SendMessage()
        {
            if (string.IsNullOrEmpty(messageInput.text) || createdAgents.Count == 0)
            {
                if (createdAgents.Count == 0)
                {
                    responseText.text = "Please create an agent first!";
                }
                return;
            }
            
            string message = messageInput.text;
            string selectedAgentId = createdAgents[agentDropdown.value];
            
            responseText.text = "Processing with function-enabled agent...";
            
            // Send message to enhanced agent
            aiSDK.SendMessageToEnhancedAgent(selectedAgentId, message, false);
            
            // Clear input
            messageInput.text = "";
        }
        
        void UpdateSystemStatus()
        {
            if (systemStatusText != null)
            {
                systemStatusText.text = aiSDK.GetEnhancedSystemStatus();
            }
        }
        
        void UpdateAvailableFunctions(EnhancedAgent agent)
        {
            if (availableFunctionsText != null && agent != null)
            {
                var functionsText = "Available Functions:\\n";
                foreach (var function in agent.AvailableFunctions)
                {
                    functionsText += $"• {function.Name}: {function.Description}\\n";
                }
                availableFunctionsText.text = functionsText;
            }
        }
        
        #region Event Handlers
        private void HandleAIResponse(string response)
        {
            responseText.text = response;
            Debug.Log($"AI Response: {response}");
        }
        
        private void HandleError(string error)
        {
            responseText.text = $"Error: {error}";
            Debug.LogError($"AI Error: {error}");
        }
        
        private void HandleAudioGenerated(AudioClip clip)
        {
            Debug.Log($"Audio generated: {clip.length} seconds");
        }
        
        private void HandleAgentCreated(UniBuddhi.Core.Interfaces.IAgent agent)
        {
            Debug.Log($"Agent created: {agent.Type}");
            UpdateSystemStatus();
        }
        #endregion
        
        #region Test Functions
        [ContextMenu("Test Calculator Function")]
        public void TestCalculatorFunction()
        {
            if (aiSDK.FunctionManager != null)
            {
                aiSDK.ExecuteFunction("add", "{\"a\": 15, \"b\": 25}", (result) =>
                {
                    Debug.Log($"Calculator test result: {result.Result} (Success: {result.Success})");
                });
            }
        }
        
        [ContextMenu("Test Weather Function")]
        public void TestWeatherFunction()
        {
            if (aiSDK.FunctionManager != null)
            {
                aiSDK.ExecuteFunction("get_current_weather", "{\"location\": \"New York\"}", (result) =>
                {
                    Debug.Log($"Weather test result: {result.Result} (Success: {result.Success})");
                });
            }
        }
        
        [ContextMenu("Test Knowledge Function")]
        public void TestKnowledgeFunction()
        {
            if (aiSDK.FunctionManager != null)
            {
                aiSDK.ExecuteFunction("search_facts", "{\"query\": \"gravity\"}", (result) =>
                {
                    Debug.Log($"Knowledge test result: {result.Result} (Success: {result.Success})");
                });
            }
        }
        
        [ContextMenu("Create Test Agents")]
        public void CreateTestAgents()
        {
            // Create different personality agents for testing
            StartCoroutine(CreateTestAgentsCoroutine());
        }
        
        private IEnumerator CreateTestAgentsCoroutine()
        {
            foreach (var personality in availablePersonalities)
            {
                var agentId = $"Test_{personality.Key.Replace(" ", "")}";
                var functionExtensions = new string[] { "Calculator", "WeatherInfo", "KnowledgeSearch" };
                
                var agent = aiSDK.CreateEnhancedAgent(agentId, defaultAgentType, personality.Value, functionExtensions);
                if (agent != null)
                {
                    createdAgents.Add(agentId);
                    Debug.Log($"Created test agent: {agentId}");
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            
            UpdateAgentDropdown();
            UpdateSystemStatus();
        }
        #endregion
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (aiSDK != null)
            {
                AISDKCore.OnAIResponse -= HandleAIResponse;
                AISDKCore.OnError -= HandleError;
                AISDKCore.OnAudioGenerated -= HandleAudioGenerated;
                AISDKCore.OnAgentCreated -= HandleAgentCreated;
            }
        }
    }
}