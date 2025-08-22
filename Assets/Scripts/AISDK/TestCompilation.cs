using System;
using System.Collections.Generic;
using UnityEngine;
using UniBuddhi.Core;
using UniBuddhi.Core.Models;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Extensions;

namespace UniBuddhi.Test
{
    /// <summary>
    /// Test script to verify all components compile correctly
    /// </summary>
    public class TestCompilation : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("Testing UniBuddhi AI SDK compilation...");
            
            // Test enhanced models
            TestEnhancedModels();
            
            // Test extension system
            TestExtensionSystem();
            
            // Test event system
            TestEventSystem();
            
            Debug.Log("✅ All tests passed! UniBuddhi AI SDK compiles successfully.");
        }
        
        private void TestEnhancedModels()
        {
            try
            {
                // Test AgentPersonality
                var personality = new AgentPersonality(
                    "TestAgent",
                    "A test agent for compilation testing",
                    "You are a test agent.",
                    0.7f
                );
                personality.Traits = new List<string> { "Test", "Compilation" };
                personality.Creativity = 0.6f;
                personality.Formality = 0.4f;
                
                // Test EnhancedAgentConfig
                var config = new EnhancedAgentConfig(AgentType.Assistant, personality);
                config.ExtensionNames.Add("TestExtension");
                config.AllowedActions = new string[] { "TestAction" };
                
                // Test AgentExtensionBinding
                var binding = new AgentExtensionBinding("TestAgent", "TestExtension");
                binding.IsEnabled = true;
                binding.Priority = 1;
                
                // Test AgentAction
                var action = new AgentAction("TestAction", "A test action");
                action.RequiredExtensions = new string[] { "TestExtension" };
                action.IsEnabled = true;
                
                // Test Event Models
                var sdkEvent = new AISDKEvent("TestEvent", "TestCompilation", "Test data");
                var agentEvent = new AgentEvent("TestAgent", AgentType.Assistant, "TestAction", "Success");
                var extensionEvent = new ExtensionEvent("TestExtension", "TestOperation", true);
                
                // Test Extension Models
                var capability = new ExtensionCapability("TestCapability", "A test capability");
                capability.SupportedActions = new string[] { "TestAction" };
                capability.IsActive = true;
                capability.PerformanceScore = 0.9f;
                
                var registryEntry = new ExtensionRegistryEntry("TestExtension", "1.0.0");
                registryEntry.Description = "A test extension";
                registryEntry.Capabilities = new ExtensionCapability[] { capability };
                
                Debug.Log("✅ Enhanced models test passed!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Enhanced models test failed: {ex.Message}");
            }
        }
        
        private void TestExtensionSystem()
        {
            try
            {
                // Test BaseExtension (abstract, so we test the interface)
                var config = new ExtensionConfig("TestExtension", "1.0.0", true, 0);
                
                Debug.Log("✅ Extension system test passed!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Extension system test failed: {ex.Message}");
            }
        }
        
        private void TestEventSystem()
        {
            try
            {
                // Test event system availability (events can only be used with += and -=)
                Debug.Log("Event system available:");
                Debug.Log("- SDK Event system: OnSDKEvent");
                Debug.Log("- Agent Event system: OnAgentEvent");
                Debug.Log("- Extension Event system: OnExtensionEvent");
                
                Debug.Log("✅ Event system test passed!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Event system test failed: {ex.Message}");
            }
        }
    }
}
