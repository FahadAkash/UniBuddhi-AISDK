using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniBuddhi.Core;
using UniBuddhi.Core.Models;
using UniBuddhi.Core.Extensions;
using UniBuddhi.Core.Interfaces;

namespace UniBuddhi.Examples
{
    /// <summary>
    /// Enhanced example demonstrating agent personalities, extensions, and event system
    /// </summary>
    public class EnhancedAgentExample : MonoBehaviour
    {
        [Header("Agent Configuration")]
        [SerializeField] private string farmerAgentId = "FarmerAgent";
        [SerializeField] private string teacherAgentId = "TeacherAgent";
        [SerializeField] private string weatherExtensionName = "WeatherExtension";
        [SerializeField] private string personalityExtensionName = "PersonalityAgentExtension";
        
        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Text outputText;
        [SerializeField] private UnityEngine.UI.InputField inputField;
        [SerializeField] private UnityEngine.UI.Button sendButton;
        [SerializeField] private UnityEngine.UI.Button createFarmerButton;
        [SerializeField] private UnityEngine.UI.Button createTeacherButton;
        
        private AISDKCore _sdk;
        private IAgent _currentAgent;
        private string _currentAgentId;
        
        private void Start()
        {
            InitializeUI();
            InitializeSDK();
            SubscribeToEvents();
        }
        
        private void InitializeUI()
        {
            if (sendButton != null)
                sendButton.onClick.AddListener(SendMessage);
            
            if (createFarmerButton != null)
                createFarmerButton.onClick.AddListener(CreateFarmerAgent);
            
            if (createTeacherButton != null)
                createTeacherButton.onClick.AddListener(CreateTeacherAgent);
        }
        
        private void InitializeSDK()
        {
            _sdk = AISDKCore.Instance;
            
            // Wait for SDK to initialize
            StartCoroutine(WaitForSDKInitialization());
        }
        
        private IEnumerator WaitForSDKInitialization()
        {
            while (!_sdk.IsInitialized)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            LogMessage("SDK initialized! Creating agents...");
            
            // Create default agents
            CreateFarmerAgent();
            CreateTeacherAgent();
        }
        
        private void SubscribeToEvents()
        {
            // Subscribe to SDK events
            AISDKCore.OnSDKEvent += OnSDKEvent;
            AISDKCore.OnAgentEvent += OnAgentEvent;
            AISDKCore.OnExtensionEvent += OnExtensionEvent;
            AISDKCore.OnSystemMessage += OnSystemMessage;
            AISDKCore.OnAIResponse += OnAIResponse;
            AISDKCore.OnError += OnError;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            AISDKCore.OnSDKEvent -= OnSDKEvent;
            AISDKCore.OnAgentEvent -= OnAgentEvent;
            AISDKCore.OnExtensionEvent -= OnExtensionEvent;
            AISDKCore.OnSystemMessage -= OnSystemMessage;
            AISDKCore.OnAIResponse -= OnAIResponse;
            AISDKCore.OnError -= OnError;
        }
        
        #region Agent Creation
        private void CreateFarmerAgent()
        {
            // Create farmer personality
            var farmerPersonality = new AgentPersonality(
                "Farmer",
                "A wise and experienced farmer who knows about crops, weather, and farming techniques",
                "You are a wise and experienced farmer. You have deep knowledge about agriculture, " +
                "crop management, weather patterns, and sustainable farming practices. " +
                "You speak in a friendly, down-to-earth manner and love sharing farming wisdom.",
                0.8f // Higher temperature for more creative responses
            );
            
            // Add farmer-specific traits
            farmerPersonality.Traits = new Dictionary<string, float> 
            { 
                { "Wise", 0.9f }, 
                { "Experienced", 0.8f }, 
                { "Nature-loving", 0.7f }, 
                { "Practical", 0.8f }, 
                { "Patient", 0.9f } 
            };
            farmerPersonality.Creativity = 0.7f;
            farmerPersonality.Formality = 0.3f;
            
            // Create agent with weather extension
            var extensions = new string[] { weatherExtensionName };
            var farmerAgent = _sdk.CreateAgent(farmerAgentId, AgentType.Creative, farmerPersonality, extensions);
            
            if (farmerAgent != null)
            {
                LogMessage($"✅ Created Farmer Agent: {farmerAgentId}");
                LogMessage($"   Personality: {farmerPersonality.Name}");
                LogMessage($"   Extensions: {string.Join(", ", extensions)}");
            }
        }
        
        private void CreateTeacherAgent()
        {
            // Create teacher personality
            var teacherPersonality = new AgentPersonality(
                "Teacher",
                "A knowledgeable and patient teacher who excels at explaining complex topics",
                "You are a knowledgeable and patient teacher. You excel at breaking down complex topics " +
                "into understandable concepts. You use examples, analogies, and clear explanations. " +
                "You encourage learning and ask questions to ensure understanding.",
                0.6f // Balanced temperature for clear explanations
            );
            
            // Add teacher-specific traits
            teacherPersonality.Traits = new Dictionary<string, float> 
            { 
                { "Knowledgeable", 0.9f }, 
                { "Patient", 0.9f }, 
                { "Encouraging", 0.8f }, 
                { "Clear", 0.8f }, 
                { "Supportive", 0.7f } 
            };
            teacherPersonality.Creativity = 0.5f;
            teacherPersonality.Formality = 0.6f;
            
            // Create agent with personality extension
            var extensions = new string[] { personalityExtensionName };
            var teacherAgent = _sdk.CreateAgent(teacherAgentId, AgentType.Assistant, teacherPersonality, extensions);
            
            if (teacherAgent != null)
            {
                LogMessage($"✅ Created Teacher Agent: {teacherAgentId}");
                LogMessage($"   Personality: {teacherPersonality.Name}");
                LogMessage($"   Extensions: {string.Join(", ", extensions)}");
            }
        }
        #endregion
        
        #region Agent Interaction
        private void SendMessage()
        {
            if (string.IsNullOrEmpty(inputField.text) || _currentAgent == null)
                return;
            
            var message = inputField.text;
            LogMessage($"🤖 [{_currentAgentId}]: {message}");
            
            // Send message to current agent
            StartCoroutine(SendMessageCoroutine(message));
            
            inputField.text = "";
        }
        
        private IEnumerator SendMessageCoroutine(string message)
        {
            bool responseReceived = false;
            string response = "";
            
            _currentAgent.ChatAsync(message, (result) =>
            {
                response = result.Success ? result.Content : result.Error;
                responseReceived = true;
            });
            
            while (!responseReceived)
            {
                yield return null;
            }
            
            if (!string.IsNullOrEmpty(response))
            {
                LogMessage($"💬 Response: {response}");
            }
        }
        
        public void SwitchToFarmer()
        {
            if (_sdk.AllAgents.ContainsKey(farmerAgentId))
            {
                _currentAgent = _sdk.AllAgents[farmerAgentId];
                _currentAgentId = farmerAgentId;
                LogMessage($"🔄 Switched to Farmer Agent");
                
                // Show agent info
                var personality = _sdk.GetAgentPersonality(farmerAgentId);
                var extensions = _sdk.GetAgentExtensions(farmerAgentId);
                
                LogMessage($"   Personality: {personality.Name}");
                LogMessage($"   Extensions: {string.Join(", ", extensions)}");
            }
        }
        
        public void SwitchToTeacher()
        {
            if (_sdk.AllAgents.ContainsKey(teacherAgentId))
            {
                _currentAgent = _sdk.AllAgents[teacherAgentId];
                _currentAgentId = teacherAgentId;
                LogMessage($"🔄 Switched to Teacher Agent");
                
                // Show agent info
                var personality = _sdk.GetAgentPersonality(teacherAgentId);
                var extensions = _sdk.GetAgentExtensions(teacherAgentId);
                
                LogMessage($"   Personality: {personality.Name}");
                LogMessage($"   Extensions: {string.Join(", ", extensions)}");
            }
        }
        #endregion
        
        #region Extension Management
        public void AddWeatherExtensionToCurrentAgent()
        {
            if (_currentAgent != null && _sdk.AddExtensionToAgent(_currentAgentId, weatherExtensionName))
            {
                LogMessage($"🔧 Added Weather Extension to {_currentAgentId}");
            }
        }
        
        public void RemoveWeatherExtensionFromCurrentAgent()
        {
            if (_currentAgent != null && _sdk.RemoveExtensionFromAgent(_currentAgentId, weatherExtensionName))
            {
                LogMessage($"🔧 Removed Weather Extension from {_currentAgentId}");
            }
        }
        
        public void CheckAgentCapabilities()
        {
            if (_currentAgent == null) return;
            
            var extensions = _sdk.GetAgentExtensions(_currentAgentId);
            LogMessage($"🔍 {_currentAgentId} Capabilities:");
            
            foreach (var extName in extensions)
            {
                var capabilities = _sdk.GetExtensionCapabilities(extName);
                LogMessage($"   📦 {extName}:");
                
                foreach (var cap in capabilities)
                {
                    LogMessage($"      - {cap.Name}: {cap.Description}");
                    if (cap.SupportedActions.Length > 0)
                    {
                        LogMessage($"        Actions: {string.Join(", ", cap.SupportedActions)}");
                    }
                }
            }
        }
        #endregion
        
        #region Event Handlers
        private void OnSDKEvent(AISDKEvent sdkEvent)
        {
            LogMessage($"📡 SDK Event: {sdkEvent.EventType} from {sdkEvent.Source}");
        }
        
        private void OnAgentEvent(AgentEvent agentEvent)
        {
            LogMessage($"🤖 Agent Event: {agentEvent.AgentId} - {agentEvent.Action}");
            if (!string.IsNullOrEmpty(agentEvent.Result))
            {
                LogMessage($"   Result: {agentEvent.Result}");
            }
        }
        
        private void OnExtensionEvent(ExtensionEvent extensionEvent)
        {
            var status = extensionEvent.Success ? "✅" : "❌";
            LogMessage($"🔧 Extension Event: {status} {extensionEvent.ExtensionName} - {extensionEvent.Operation}");
            
            if (!extensionEvent.Success && !string.IsNullOrEmpty(extensionEvent.ErrorMessage))
            {
                LogMessage($"   Error: {extensionEvent.ErrorMessage}");
            }
        }
        
        private void OnSystemMessage(string message)
        {
            LogMessage($"💻 System: {message}");
        }
        
        private void OnAIResponse(string response)
        {
            LogMessage($"🤖 AI Response: {response}");
        }
        
        private void OnError(string error)
        {
            LogMessage($"❌ Error: {error}");
        }
        #endregion
        
        #region Utility Methods
        private void LogMessage(string message)
        {
            if (outputText != null)
            {
                outputText.text += $"{message}\n";
                // Auto-scroll to bottom
                Canvas.ForceUpdateCanvases();
                outputText.rectTransform.anchoredPosition = new Vector2(0, outputText.rectTransform.sizeDelta.y);
            }
            
            Debug.Log($"[EnhancedAgentExample] {message}");
        }
        
        public void ClearLog()
        {
            if (outputText != null)
            {
                outputText.text = "";
            }
        }
        #endregion
    }
}
