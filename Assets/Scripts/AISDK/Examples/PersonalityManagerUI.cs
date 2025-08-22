using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AISDK.Core;
using AISDK.Core.Extensions;
using AISDK.Core.Models;

namespace AISDK.Examples
{
    /// <summary>
    /// UI Controller for managing personality agents
    /// </summary>
    public class PersonalityManagerUI : MonoBehaviour
    {
        [Header("UI References")]
        public Dropdown personalityDropdown;
        public Text personalityInfoText;
        public Text currentPersonalityText;
        public Button switchPersonalityButton;
        public Toggle overrideSystemPromptToggle;
        public Toggle addPersonalityContextToggle;
        public Toggle showPersonalityInResponseToggle;
        
        [Header("Chat Interface")]
        public InputField messageInput;
        public Text chatOutput;
        public Button sendMessageButton;
        public ScrollRect chatScrollRect;
        
        [Header("Personality Creation")]
        public GameObject personalityCreationPanel;
        public InputField newPersonalityNameInput;
        public InputField newPersonalityTraitsInput;
        public InputField newPersonalitySystemPromptInput;
        public InputField newPersonalityResponseStyleInput;
        public Button createPersonalityButton;
        public Button showCreatePanelButton;
        public Button hideCreatePanelButton;
        
        private AISDKCore aiSDK;
        private PersonalityAgentExtension personalityExtension;
        private List<PersonalityAgent> availablePersonalities;
        
        void Start()
        {
            InitializeComponents();
            SetupUI();
            LoadPersonalities();
        }
        
        void InitializeComponents()
        {
            // Get AI SDK
            aiSDK = FindFirstObjectByType<AISDKCore>();
            if (aiSDK == null)
            {
                Debug.LogError("AISDKCore not found! Please add it to the scene.");
                return;
            }
            
            // Get personality extension
            personalityExtension = FindFirstObjectByType<PersonalityAgentExtension>();
            if (personalityExtension == null)
            {
                Debug.LogWarning("PersonalityAgentExtension not found. Creating one...");
                CreatePersonalityExtension();
            }
            
            // Subscribe to events
            AISDKCore.OnAIResponse += OnAIResponse;
            AISDKCore.OnError += OnAIError;
            
            if (personalityExtension != null)
            {
                personalityExtension.OnPersonalityChanged += OnPersonalityChanged;
                personalityExtension.OnPersonalityContextAdded += OnPersonalityContextAdded;
            }
        }
        
        void CreatePersonalityExtension()
        {
            GameObject extensionGO = new GameObject("PersonalityAgentExtension");
            extensionGO.transform.SetParent(aiSDK.transform);
            personalityExtension = extensionGO.AddComponent<PersonalityAgentExtension>();
            
            // Add to SDK's extension list if possible
            if (aiSDK != null)
            {
                aiSDK.AddExtension(personalityExtension);
            }
        }
        
        void SetupUI()
        {
            // Setup buttons
            switchPersonalityButton?.onClick.AddListener(SwitchPersonality);
            sendMessageButton?.onClick.AddListener(SendMessage);
            createPersonalityButton?.onClick.AddListener(CreateCustomPersonality);
            showCreatePanelButton?.onClick.AddListener(() => personalityCreationPanel?.SetActive(true));
            hideCreatePanelButton?.onClick.AddListener(() => personalityCreationPanel?.SetActive(false));
            
            // Setup dropdown
            personalityDropdown?.onValueChanged.AddListener(OnPersonalityDropdownChanged);
            
            // Setup toggles
            overrideSystemPromptToggle?.onValueChanged.AddListener(OnOverrideSystemPromptChanged);
            addPersonalityContextToggle?.onValueChanged.AddListener(OnAddPersonalityContextChanged);
            showPersonalityInResponseToggle?.onValueChanged.AddListener(OnShowPersonalityInResponseChanged);
            
            // Hide creation panel initially
            personalityCreationPanel?.SetActive(false);
            
            // Initial chat message
            if (chatOutput != null)
            {
                chatOutput.text = "🤖 Personality Manager Ready!\nSelect a personality and start chatting!\n\n";
            }
        }
        
        void LoadPersonalities()
        {
            if (personalityExtension == null) return;
            
            availablePersonalities = personalityExtension.GetAllPersonalities();
            PopulatePersonalityDropdown();
            UpdatePersonalityInfo();
            UpdateCurrentPersonalityDisplay();
        }
        
        void PopulatePersonalityDropdown()
        {
            if (personalityDropdown == null || availablePersonalities == null) return;
            
            personalityDropdown.ClearOptions();
            
            List<string> options = new List<string>();
            foreach (var personality in availablePersonalities)
            {
                options.Add($"{personality.Name} ({personality.Type})");
            }
            
            personalityDropdown.AddOptions(options);
        }
        
        void UpdatePersonalityInfo()
        {
            if (personalityInfoText == null || availablePersonalities == null) return;
            
            int selectedIndex = personalityDropdown.value;
            if (selectedIndex >= 0 && selectedIndex < availablePersonalities.Count)
            {
                var personality = availablePersonalities[selectedIndex];
                
                string info = $"<b>{personality.Name}</b>\n\n";
                info += $"<b>Traits:</b> {string.Join(", ", personality.PersonalityTraits)}\n\n";
                info += $"<b>Response Style:</b>\n{personality.ResponseStyle}\n\n";
                
                if (personality.PreferredTopics.Count > 0)
                {
                    info += $"<b>Preferred Topics:</b>\n{string.Join(", ", personality.PreferredTopics)}\n\n";
                }
                
                if (personality.AvoidedTopics.Count > 0)
                {
                    info += $"<b>Avoided Topics:</b>\n{string.Join(", ", personality.AvoidedTopics)}\n\n";
                }
                
                info += $"<b>System Prompt:</b>\n<i>{personality.SystemPrompt.Substring(0, Mathf.Min(200, personality.SystemPrompt.Length))}...</i>";
                
                personalityInfoText.text = info;
            }
        }
        
        void UpdateCurrentPersonalityDisplay()
        {
            if (currentPersonalityText == null || personalityExtension == null) return;
            
            var currentPersonality = personalityExtension.GetCurrentPersonality();
            if (currentPersonality != null)
            {
                currentPersonalityText.text = $"Current: <b>{currentPersonality.Name}</b>";
            }
            else
            {
                currentPersonalityText.text = "Current: <b>None Selected</b>";
            }
        }
        
        #region UI Event Handlers
        
        void OnPersonalityDropdownChanged(int index)
        {
            UpdatePersonalityInfo();
        }
        
        void SwitchPersonality()
        {
            if (personalityExtension == null || availablePersonalities == null) return;
            
            int selectedIndex = personalityDropdown.value;
            if (selectedIndex >= 0 && selectedIndex < availablePersonalities.Count)
            {
                var selectedPersonality = availablePersonalities[selectedIndex];
                personalityExtension.SetPersonality(selectedPersonality.Type);
                
                // Add chat message about personality switch
                AddToChatOutput($"🎭 <color=orange>Switched to: {selectedPersonality.Name}</color>\n");
            }
        }
        
        void SendMessage()
        {
            if (aiSDK == null || messageInput == null || string.IsNullOrEmpty(messageInput.text)) return;
            
            string message = messageInput.text;
            AddToChatOutput($"<color=cyan>You:</color> {message}\n");
            
            // Send message to AI
            aiSDK.SendMessage(message, AgentType.Assistant, false);
            
            // Clear input
            messageInput.text = "";
        }
        
        void CreateCustomPersonality()
        {
            if (personalityExtension == null) return;
            
            // Get input values
            string name = newPersonalityNameInput?.text ?? "Custom Personality";
            string traits = newPersonalityTraitsInput?.text ?? "helpful, friendly";
            string systemPrompt = newPersonalitySystemPromptInput?.text ?? "You are a helpful AI assistant.";
            string responseStyle = newPersonalityResponseStyleInput?.text ?? "Be helpful and friendly.";
            
            // Create new personality
            var newPersonality = new PersonalityAgent
            {
                Type = PersonalityType.Custom,
                Name = name,
                SystemPrompt = systemPrompt,
                ResponseStyle = responseStyle,
                PersonalityTraits = new List<string>(traits.Split(','))
            };
            
            // Add to extension
            personalityExtension.AddCustomPersonality(newPersonality);
            
            // Refresh UI
            LoadPersonalities();
            
            // Hide creation panel
            personalityCreationPanel?.SetActive(false);
            
            // Clear inputs
            ClearCreationInputs();
            
            AddToChatOutput($"✨ <color=green>Created new personality: {name}</color>\n");
        }
        
        void ClearCreationInputs()
        {
            if (newPersonalityNameInput != null) newPersonalityNameInput.text = "";
            if (newPersonalityTraitsInput != null) newPersonalityTraitsInput.text = "";
            if (newPersonalitySystemPromptInput != null) newPersonalitySystemPromptInput.text = "";
            if (newPersonalityResponseStyleInput != null) newPersonalityResponseStyleInput.text = "";
        }
        
        void OnOverrideSystemPromptChanged(bool value)
        {
            // Update extension settings
            Debug.Log($"Override System Prompt: {value}");
        }
        
        void OnAddPersonalityContextChanged(bool value)
        {
            // Update extension settings
            Debug.Log($"Add Personality Context: {value}");
        }
        
        void OnShowPersonalityInResponseChanged(bool value)
        {
            // Update extension settings
            Debug.Log($"Show Personality in Response: {value}");
        }
        
        #endregion
        
        #region AI Event Handlers
        
        void OnAIResponse(string response)
        {
            var currentPersonality = personalityExtension?.GetCurrentPersonality();
            string personalityName = currentPersonality?.Name ?? "AI";
            
            AddToChatOutput($"<color=lime>{personalityName}:</color> {response}\n\n");
        }
        
        void OnAIError(string error)
        {
            AddToChatOutput($"<color=red>Error:</color> {error}\n");
        }
        
        void OnPersonalityChanged(PersonalityType newPersonality)
        {
            UpdateCurrentPersonalityDisplay();
            Debug.Log($"Personality changed to: {newPersonality}");
        }
        
        void OnPersonalityContextAdded(string context)
        {
            Debug.Log($"Personality context added: {context.Substring(0, Mathf.Min(100, context.Length))}...");
        }
        
        #endregion
        
        #region Utility Methods
        
        void AddToChatOutput(string text)
        {
            if (chatOutput == null) return;
            
            chatOutput.text += text;
            
            // Auto-scroll to bottom
            if (chatScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                chatScrollRect.verticalNormalizedPosition = 0f;
            }
        }
        
        #endregion
        
        #region Public Methods for UI Integration
        
        /// <summary>
        /// Switch to a specific personality by name
        /// </summary>
        public void SwitchToPersonality(string personalityName)
        {
            if (personalityExtension == null) return;
            
            var personalities = personalityExtension.GetAllPersonalities();
            var targetPersonality = personalities.Find(p => p.Name.Equals(personalityName, System.StringComparison.OrdinalIgnoreCase));
            
            if (targetPersonality != null)
            {
                personalityExtension.SetPersonality(targetPersonality.Type);
            }
        }
        
        /// <summary>
        /// Get current personality name
        /// </summary>
        public string GetCurrentPersonalityName()
        {
            var currentPersonality = personalityExtension?.GetCurrentPersonality();
            return currentPersonality?.Name ?? "None";
        }
        
        /// <summary>
        /// Clear chat output
        /// </summary>
        public void ClearChat()
        {
            if (chatOutput != null)
            {
                chatOutput.text = "🤖 Chat cleared!\n\n";
            }
        }
        
        #endregion
    }
}
