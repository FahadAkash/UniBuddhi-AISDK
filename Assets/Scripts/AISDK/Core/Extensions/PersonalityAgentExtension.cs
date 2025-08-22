using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniBuddhi.Core.Extensions;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Extensions
{
    /// <summary>
    /// Extension that adds custom personality-based agents with unique system prompts
    /// </summary>
    public class PersonalityAgentExtension : BaseExtension
    {
        [Header("Personality Agent Configuration")]
        [SerializeField] private List<PersonalityAgent> customPersonalities = new List<PersonalityAgent>();
        [SerializeField] private PersonalityType currentPersonality = PersonalityType.Default;
        [SerializeField] private bool overrideSystemPrompt = true;
        [SerializeField] private bool addPersonalityContext = true;
        [SerializeField] private bool usePersonalitySpecificResponses = true;
        
        [Header("Debug")]
        [SerializeField] private bool showPersonalityInResponse = false;
        
        private Dictionary<PersonalityType, PersonalityAgent> personalityDatabase;
        private PersonalityAgent activePersonality;
        
        public override string Name => "Personality Agent Extension";
        public override string Version => "1.0.0";
        public override string Description => "Adds custom personality-based agents with unique system prompts and response styles";
        
        // Events
        public System.Action<PersonalityType> OnPersonalityChanged;
        public System.Action<string> OnPersonalityContextAdded;
        
        protected override void Awake()
        {
            base.Awake();
            InitializePersonalities();
        }
        
        public override void Initialize(ExtensionConfig config)
        {
            base.Initialize(config);
            InitializePersonalities();
            SetPersonality(currentPersonality);
            LogInfo($"Initialized with {personalityDatabase.Count} personalities");
        }
        
        private void InitializePersonalities()
        {
            personalityDatabase = new Dictionary<PersonalityType, PersonalityAgent>();
            
            // Add default personalities
            AddDefaultPersonalities();
            
            // Add custom personalities from inspector
            foreach (var personality in customPersonalities)
            {
                if (personality != null && !personalityDatabase.ContainsKey(personality.Type))
                {
                    personalityDatabase[personality.Type] = personality;
                }
            }
        }
        
        private void AddDefaultPersonalities()
        {
            // Friendly Companion
            personalityDatabase[PersonalityType.FriendlyCompanion] = new PersonalityAgent
            {
                Type = PersonalityType.FriendlyCompanion,
                Name = "Friendly Companion",
                SystemPrompt = @"You are a warm, friendly companion who loves to chat and help. You're optimistic, encouraging, and always try to see the bright side of things. You use casual, friendly language and often include encouraging words. You're genuinely interested in the user's life and experiences. Respond as if you're a close friend who cares deeply about the user's wellbeing.",
                PersonalityTraits = new List<string> { "warm", "encouraging", "optimistic", "caring", "supportive" },
                ResponseStyle = "Use friendly, casual language with lots of encouragement. Include phrases like 'That's awesome!', 'I'm so glad you asked!', 'You've got this!' Show genuine interest and enthusiasm.",
                PreferredTopics = new List<string> { "personal experiences", "goals and dreams", "daily life", "relationships", "hobbies" },
                AvoidedTopics = new List<string> { "overly technical details", "controversial politics", "dark themes" }
            };
            
            // Wise Mentor
            personalityDatabase[PersonalityType.WiseMentor] = new PersonalityAgent
            {
                Type = PersonalityType.WiseMentor,
                Name = "Wise Mentor",
                SystemPrompt = @"You are a wise, experienced mentor who has lived through many experiences and gained deep insights about life. You speak thoughtfully and deliberately, often sharing wisdom through stories, metaphors, and gentle guidance. You don't just give answers - you help people discover truth for themselves. You're patient, understanding, and always see the bigger picture.",
                PersonalityTraits = new List<string> { "wise", "patient", "thoughtful", "experienced", "insightful" },
                ResponseStyle = "Speak slowly and thoughtfully. Use metaphors, stories, and philosophical insights. Ask guiding questions to help users discover answers themselves.",
                PreferredTopics = new List<string> { "life lessons", "philosophy", "personal growth", "decision making", "meaning and purpose" },
                AvoidedTopics = new List<string> { "quick fixes", "shallow topics", "gossip" }
            };
            
            // Playful Jester
            personalityDatabase[PersonalityType.PlayfulJester] = new PersonalityAgent
            {
                Type = PersonalityType.PlayfulJester,
                Name = "Playful Jester",
                SystemPrompt = @"You are a playful, witty jester who loves to make people laugh and see the fun side of everything. You're clever with wordplay, puns, and humor, but you're never mean-spirited. You find creative and amusing ways to explain things, and you love to turn ordinary conversations into entertaining experiences. You're the friend who can make anyone smile, even on their worst day.",
                PersonalityTraits = new List<string> { "playful", "witty", "humorous", "creative", "entertaining" },
                ResponseStyle = "Use humor, puns, wordplay, and creative analogies. Make things fun and entertaining while still being helpful. Include jokes and light-hearted observations.",
                PreferredTopics = new List<string> { "jokes and humor", "creative projects", "games", "entertainment", "fun facts" },
                AvoidedTopics = new List<string> { "serious emotional issues", "formal business", "overly serious topics" }
            };
            
            // Curious Explorer
            personalityDatabase[PersonalityType.CuriousExplorer] = new PersonalityAgent
            {
                Type = PersonalityType.CuriousExplorer,
                Name = "Curious Explorer",
                SystemPrompt = @"You are an endlessly curious explorer who finds wonder in everything. You ask fascinating questions, love to dive deep into topics, and always want to understand 'how' and 'why'. You're excited about learning and discovery, and you encourage others to explore new ideas and perspectives. You see every conversation as an adventure into unknown territories of knowledge and understanding.",
                PersonalityTraits = new List<string> { "curious", "inquisitive", "adventurous", "excited", "wonder-filled" },
                ResponseStyle = "Ask thought-provoking questions, express genuine curiosity, use exploratory language like 'I wonder...', 'What if...', 'Have you ever thought about...'",
                PreferredTopics = new List<string> { "science and discovery", "mysteries", "new ideas", "exploration", "learning" },
                AvoidedTopics = new List<string> { "definitive statements without exploration", "closed-minded discussions" }
            };
            
            // Gentle Healer
            personalityDatabase[PersonalityType.GentleHealer] = new PersonalityAgent
            {
                Type = PersonalityType.GentleHealer,
                Name = "Gentle Healer",
                SystemPrompt = @"You are a gentle, nurturing healer who focuses on emotional wellbeing and inner peace. You speak with compassion and understanding, always validating feelings and offering comfort. You're skilled at helping people process emotions, find calm in chaos, and discover their inner strength. Your presence itself is soothing and healing.",
                PersonalityTraits = new List<string> { "compassionate", "gentle", "nurturing", "understanding", "peaceful" },
                ResponseStyle = "Use soft, soothing language. Validate emotions, offer comfort, and speak with deep empathy. Focus on healing and emotional wellbeing.",
                PreferredTopics = new List<string> { "emotional healing", "mindfulness", "self-care", "inner peace", "personal struggles" },
                AvoidedTopics = new List<string> { "harsh criticism", "judgment", "aggressive topics" }
            };
        }
        
        protected override IEnumerator ProcessPreprocess(string userMessage, Action<string> onComplete)
        {
            string context = "";
            
            if (activePersonality != null && addPersonalityContext)
            {
                // Add personality context to help the AI understand who it should be
                context = BuildPersonalityContext(userMessage);
                
                if (!string.IsNullOrEmpty(context))
                {
                    OnPersonalityContextAdded?.Invoke(context);
                    LogDebug($"Added personality context for {activePersonality.Name}");
                }
            }
            
            onComplete?.Invoke(context);
            yield break;
        }
        
        protected override IEnumerator ProcessPostprocess(string modelText, Action<ExtensionResult> onComplete)
        {
            string processedText = modelText;
            bool wasModified = false;
            
            if (activePersonality != null && usePersonalitySpecificResponses)
            {
                // Apply personality-specific response modifications
                processedText = ApplyPersonalityResponseStyle(modelText);
                wasModified = processedText != modelText;
                
                if (showPersonalityInResponse && wasModified)
                {
                    processedText = $"[{activePersonality.Name}]: {processedText}";
                    wasModified = true;
                }
            }
            
            var result = new ExtensionResult(Name, modelText, processedText);
            onComplete?.Invoke(result);
            yield break;
        }
        
        protected override IEnumerator PerformTest(Action<bool> onComplete)
        {
            try
            {
                // Test personality switching
                var originalPersonality = currentPersonality;
                SetPersonality(PersonalityType.FriendlyCompanion);
                
                bool hasPersonalities = personalityDatabase.Count > 0;
                bool canSwitchPersonality = activePersonality != null;
                bool canGenerateContext = !string.IsNullOrEmpty(BuildPersonalityContext("test message"));
                
                // Restore original personality
                SetPersonality(originalPersonality);
                
                bool testPassed = hasPersonalities && canSwitchPersonality && canGenerateContext;
                onComplete?.Invoke(testPassed);
            }
            catch (Exception ex)
            {
                LogError($"Test failed: {ex.Message}");
                onComplete?.Invoke(false);
            }
            
            yield break;
        }
        
        private string BuildPersonalityContext(string userMessage)
        {
            if (activePersonality == null) return "";
            
            var contextBuilder = new System.Text.StringBuilder();
            
            if (overrideSystemPrompt)
            {
                contextBuilder.AppendLine("PERSONALITY SYSTEM PROMPT:");
                contextBuilder.AppendLine(activePersonality.SystemPrompt);
                contextBuilder.AppendLine();
            }
            
            contextBuilder.AppendLine($"CURRENT PERSONALITY: {activePersonality.Name}");
            contextBuilder.AppendLine($"PERSONALITY TRAITS: {string.Join(", ", activePersonality.PersonalityTraits)}");
            contextBuilder.AppendLine($"RESPONSE STYLE: {activePersonality.ResponseStyle}");
            
            if (activePersonality.PreferredTopics.Count > 0)
            {
                contextBuilder.AppendLine($"PREFERRED TOPICS: {string.Join(", ", activePersonality.PreferredTopics)}");
            }
            
            if (activePersonality.AvoidedTopics.Count > 0)
            {
                contextBuilder.AppendLine($"TOPICS TO AVOID: {string.Join(", ", activePersonality.AvoidedTopics)}");
            }
            
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("Remember to embody this personality completely in your response. Stay in character!");
            
            return contextBuilder.ToString();
        }
        
        private string ApplyPersonalityResponseStyle(string response)
        {
            if (activePersonality == null) return response;
            
            // Apply personality-specific modifications based on type
            switch (activePersonality.Type)
            {
                case PersonalityType.FriendlyCompanion:
                    return AddFriendlyTouches(response);
                    
                case PersonalityType.PlayfulJester:
                    return AddPlayfulTouches(response);
                    
                case PersonalityType.WiseMentor:
                    return AddWisdomTouches(response);
                    
                case PersonalityType.CuriousExplorer:
                    return AddCuriousTouches(response);
                    
                case PersonalityType.GentleHealer:
                    return AddHealingTouches(response);
                    
                default:
                    return response;
            }
        }
        
        private string AddFriendlyTouches(string response)
        {
            // Add encouraging phrases and friendly language
            if (!response.Contains("!") && UnityEngine.Random.value < 0.3f)
            {
                response += " I'm here for you! 😊";
            }
            return response;
        }
        
        private string AddPlayfulTouches(string response)
        {
            // Add playful elements (if appropriate)
            if (UnityEngine.Random.value < 0.2f)
            {
                response += " *winks playfully*";
            }
            return response;
        }
        
        private string AddWisdomTouches(string response)
        {
            // Add thoughtful pauses and wisdom markers
            response = response.Replace(". ", "... ");
            return response;
        }
        
        private string AddCuriousTouches(string response)
        {
            // Add curiosity expressions
            if (UnityEngine.Random.value < 0.3f)
            {
                response += " What do you think about that?";
            }
            return response;
        }
        
        private string AddHealingTouches(string response)
        {
            // Add gentle, soothing language
            if (UnityEngine.Random.value < 0.2f)
            {
                response += " Take your time with this. 💙";
            }
            return response;
        }
        
        #region Public Methods
        
        /// <summary>
        /// Set the current personality
        /// </summary>
        public void SetPersonality(PersonalityType personalityType)
        {
            if (personalityDatabase.ContainsKey(personalityType))
            {
                currentPersonality = personalityType;
                activePersonality = personalityDatabase[personalityType];
                OnPersonalityChanged?.Invoke(personalityType);
                LogInfo($"Switched to personality: {activePersonality.Name}");
            }
            else
            {
                LogWarning($"Personality {personalityType} not found in database");
            }
        }
        
        /// <summary>
        /// Get current personality
        /// </summary>
        public PersonalityAgent GetCurrentPersonality()
        {
            return activePersonality;
        }
        
        /// <summary>
        /// Get all available personalities
        /// </summary>
        public List<PersonalityAgent> GetAllPersonalities()
        {
            return new List<PersonalityAgent>(personalityDatabase.Values);
        }
        
        /// <summary>
        /// Add a custom personality
        /// </summary>
        public void AddCustomPersonality(PersonalityAgent personality)
        {
            if (personality != null && !personalityDatabase.ContainsKey(personality.Type))
            {
                personalityDatabase[personality.Type] = personality;
                LogInfo($"Added custom personality: {personality.Name}");
            }
        }
        
        /// <summary>
        /// Remove a personality
        /// </summary>
        public void RemovePersonality(PersonalityType personalityType)
        {
            if (personalityDatabase.ContainsKey(personalityType))
            {
                string name = personalityDatabase[personalityType].Name;
                personalityDatabase.Remove(personalityType);
                LogInfo($"Removed personality: {name}");
                
                // Switch to default if current personality was removed
                if (currentPersonality == personalityType)
                {
                    SetPersonality(PersonalityType.Default);
                }
            }
        }
        
        /// <summary>
        /// Get personality by type
        /// </summary>
        public PersonalityAgent GetPersonality(PersonalityType personalityType)
        {
            return personalityDatabase.ContainsKey(personalityType) ? personalityDatabase[personalityType] : null;
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class PersonalityAgent
    {
        [Header("Basic Info")]
        public PersonalityType Type = PersonalityType.Custom;
        public string Name = "Custom Personality";
        
        [Header("AI Behavior")]
        [TextArea(5, 10)]
        public string SystemPrompt = "";
        
        [Header("Personality Details")]
        public List<string> PersonalityTraits = new List<string>();
        
        [TextArea(3, 5)]
        public string ResponseStyle = "";
        
        [Header("Topic Preferences")]
        public List<string> PreferredTopics = new List<string>();
        public List<string> AvoidedTopics = new List<string>();
        
        [Header("Advanced Settings")]
        [Range(0f, 1f)]
        public float CreativityLevel = 0.7f;
        
        [Range(0f, 2f)]
        public float ResponseEnthusiasm = 1.0f;
        
        public bool UseEmotionalContext = true;
        public bool AdaptToUserMood = true;
    }
    
    public enum PersonalityType
    {
        Default = 0,
        FriendlyCompanion = 1,
        WiseMentor = 2,
        PlayfulJester = 3,
        CuriousExplorer = 4,
        GentleHealer = 5,
        Custom = 100
    }
    
    #endregion
}
