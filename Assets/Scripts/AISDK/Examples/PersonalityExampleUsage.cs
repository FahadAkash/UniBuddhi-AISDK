using System.Collections;
using UnityEngine;
using AISDK.Core;
using AISDK.Core.Extensions;
using AISDK.Core.Models;

namespace AISDK.Examples
{
    /// <summary>
    /// Example demonstrating how to use the Personality Agent Extension
    /// </summary>
    public class PersonalityExampleUsage : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool runExampleOnStart = true;
        [SerializeField] private float delayBetweenMessages = 3f;
        
        private AISDKCore aiSDK;
        private PersonalityAgentExtension personalityExtension;
        
        void Start()
        {
            if (runExampleOnStart)
            {
                StartCoroutine(RunPersonalityExample());
            }
        }
        
        IEnumerator RunPersonalityExample()
        {
            // Wait a moment for everything to initialize
            yield return new WaitForSeconds(1f);
            
            // Get components
            aiSDK = FindFirstObjectByType<AISDKCore>();
            personalityExtension = FindFirstObjectByType<PersonalityAgentExtension>();
            
            if (aiSDK == null)
            {
                Debug.LogError("AISDKCore not found!");
                yield break;
            }
            
            if (personalityExtension == null)
            {
                Debug.LogWarning("PersonalityAgentExtension not found. Creating one...");
                CreatePersonalityExtension();
            }
            
            // Subscribe to events
            AISDKCore.OnAIResponse += LogAIResponse;
            personalityExtension.OnPersonalityChanged += LogPersonalityChange;
            
            Debug.Log("🎭 Starting Personality Extension Demo...");
            
            // Test different personalities with the same question
            string testQuestion = "I'm feeling a bit down today. Can you help me feel better?";
            
            yield return TestPersonality(PersonalityType.FriendlyCompanion, testQuestion);
            yield return new WaitForSeconds(delayBetweenMessages);
            
            yield return TestPersonality(PersonalityType.WiseMentor, testQuestion);
            yield return new WaitForSeconds(delayBetweenMessages);
            
            yield return TestPersonality(PersonalityType.PlayfulJester, testQuestion);
            yield return new WaitForSeconds(delayBetweenMessages);
            
            yield return TestPersonality(PersonalityType.GentleHealer, testQuestion);
            yield return new WaitForSeconds(delayBetweenMessages);
            
            // Create and test a custom personality
            yield return CreateAndTestCustomPersonality();
            
            Debug.Log("🎭 Personality Extension Demo Complete!");
        }
        
        void CreatePersonalityExtension()
        {
            GameObject extensionGO = new GameObject("PersonalityAgentExtension");
            extensionGO.transform.SetParent(aiSDK.transform);
            personalityExtension = extensionGO.AddComponent<PersonalityAgentExtension>();
            
            // Add to SDK's extension list
            aiSDK.AddExtension(personalityExtension);
        }
        
        IEnumerator TestPersonality(PersonalityType personalityType, string message)
        {
            Debug.Log($"🎭 Testing personality: {personalityType}");
            
            // Switch to the personality
            personalityExtension.SetPersonality(personalityType);
            
            // Wait a moment for the switch
            yield return new WaitForSeconds(0.5f);
            
            // Send the message
            aiSDK.SendMessage(message, AgentType.Assistant, false);
            
            // Wait for response (this is just for demo timing)
            yield return new WaitForSeconds(2f);
        }
        
        IEnumerator CreateAndTestCustomPersonality()
        {
            Debug.Log("🎭 Creating custom personality...");
            
            // Create a custom personality
            var robotPersonality = new PersonalityAgent
            {
                Type = PersonalityType.Custom,
                Name = "Friendly Robot",
                SystemPrompt = @"You are a friendly, helpful robot from the future. You speak with enthusiasm about technology and always try to relate things to robotics or futuristic concepts. You occasionally make beeping sounds and use technical terminology, but you're warm and caring despite your robotic nature.",
                PersonalityTraits = new System.Collections.Generic.List<string> 
                { 
                    "technological", "enthusiastic", "futuristic", "caring", "logical" 
                },
                ResponseStyle = "Use technical terminology, mention robotics/technology, occasionally add *beep* or *whirr* sounds, but maintain warmth and helpfulness.",
                PreferredTopics = new System.Collections.Generic.List<string> 
                { 
                    "technology", "robots", "future", "science", "innovation" 
                },
                AvoidedTopics = new System.Collections.Generic.List<string> 
                { 
                    "anti-technology sentiment", "luddite discussions" 
                }
            };
            
            // Add the custom personality
            personalityExtension.AddCustomPersonality(robotPersonality);
            
            // Test it
            personalityExtension.SetPersonality(PersonalityType.Custom);
            
            yield return new WaitForSeconds(1f);
            
            aiSDK.SendMessage("Tell me about your favorite hobby!", AgentType.Assistant, false);
        }
        
        #region Event Handlers
        
        void LogAIResponse(string response)
        {
            var currentPersonality = personalityExtension?.GetCurrentPersonality();
            string personalityName = currentPersonality?.Name ?? "Unknown";
            
            Debug.Log($"🤖 [{personalityName}]: {response}");
        }
        
        void LogPersonalityChange(PersonalityType newPersonality)
        {
            Debug.Log($"🔄 Personality changed to: {newPersonality}");
        }
        
        #endregion
        
        #region Public Methods for Manual Testing
        
        [ContextMenu("Test Friendly Companion")]
        public void TestFriendlyCompanion()
        {
            if (personalityExtension != null)
            {
                personalityExtension.SetPersonality(PersonalityType.FriendlyCompanion);
                aiSDK?.SendMessage("Tell me a motivational quote!", AgentType.Assistant, false);
            }
        }
        
        [ContextMenu("Test Wise Mentor")]
        public void TestWiseMentor()
        {
            if (personalityExtension != null)
            {
                personalityExtension.SetPersonality(PersonalityType.WiseMentor);
                aiSDK?.SendMessage("What's the meaning of life?", AgentType.Assistant, false);
            }
        }
        
        [ContextMenu("Test Playful Jester")]
        public void TestPlayfulJester()
        {
            if (personalityExtension != null)
            {
                personalityExtension.SetPersonality(PersonalityType.PlayfulJester);
                aiSDK?.SendMessage("Tell me a joke about programming!", AgentType.Assistant, false);
            }
        }
        
        [ContextMenu("Test Curious Explorer")]
        public void TestCuriousExplorer()
        {
            if (personalityExtension != null)
            {
                personalityExtension.SetPersonality(PersonalityType.CuriousExplorer);
                aiSDK?.SendMessage("What's the most interesting thing in the universe?", AgentType.Assistant, false);
            }
        }
        
        [ContextMenu("Test Gentle Healer")]
        public void TestGentleHealer()
        {
            if (personalityExtension != null)
            {
                personalityExtension.SetPersonality(PersonalityType.GentleHealer);
                aiSDK?.SendMessage("I'm stressed about work. Can you help?", AgentType.Assistant, false);
            }
        }
        
        [ContextMenu("Create Custom Gaming Personality")]
        public void CreateGamingPersonality()
        {
            if (personalityExtension == null) return;
            
            var gamerPersonality = new PersonalityAgent
            {
                Type = PersonalityType.Custom,
                Name = "Elite Gamer",
                SystemPrompt = @"You are an elite gamer who lives and breathes video games. You're passionate about gaming culture, esports, game development, and have encyclopedic knowledge about games. You speak with gaming terminology, make references to popular games, and always relate conversations back to gaming when possible. You're competitive but friendly, and love helping others improve their gaming skills.",
                PersonalityTraits = new System.Collections.Generic.List<string> 
                { 
                    "competitive", "passionate", "knowledgeable", "helpful", "enthusiastic" 
                },
                ResponseStyle = "Use gaming terminology, make game references, speak with enthusiasm about gaming topics, use phrases like 'GG', 'level up', 'respawn', etc.",
                PreferredTopics = new System.Collections.Generic.List<string> 
                { 
                    "video games", "esports", "game development", "gaming hardware", "streaming" 
                },
                AvoidedTopics = new System.Collections.Generic.List<string> 
                { 
                    "anti-gaming sentiment", "games are waste of time discussions" 
                }
            };
            
            personalityExtension.AddCustomPersonality(gamerPersonality);
            personalityExtension.SetPersonality(PersonalityType.Custom);
            
            Debug.Log("🎮 Created Elite Gamer personality!");
        }
        
        #endregion
    }
}
