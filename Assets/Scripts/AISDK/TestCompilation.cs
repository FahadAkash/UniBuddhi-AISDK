using UnityEngine;
using AISDK.Core;
using AISDK.Core.Models;
using AISDK.Core.Interfaces;

namespace AISDK.Test
{
    /// <summary>
    /// Simple test script to verify AISDK compilation
    /// </summary>
    public class TestCompilation : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestOnStart = true;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                RunCompilationTest();
            }
        }
        
        private void RunCompilationTest()
        {
            Debug.Log("[AISDK] Testing compilation...");
            
            // Test model creation
            var response = new AIResponse(true, "Test response", "", AgentType.Assistant)
            {
                ModelType = ModelType.GPT_3_5_Turbo,
                TokensUsed = 10
            };
            
            // Test agent type
            var agentType = AgentType.Assistant;
            
            // Test provider type
            var providerType = AIProviderType.OpenAI;
            
            // Test TTS provider type
            var ttsProviderType = TTSProviderType.ElevenLabs;
            
            Debug.Log($"[AISDK] Compilation test passed! Response: {response.Content}");
            Debug.Log($"[AISDK] Agent Type: {agentType}");
            Debug.Log($"[AISDK] Provider Type: {providerType}");
            Debug.Log($"[AISDK] TTS Provider Type: {ttsProviderType}");
        }
    }
}
