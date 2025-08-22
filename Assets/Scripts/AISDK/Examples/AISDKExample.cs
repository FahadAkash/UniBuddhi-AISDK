using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AISDK.Core;
using AISDK.Core.Models;
using AISDK.Core.Extensions;
using AISDK.Core.Interfaces;

namespace AISDK.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the AI SDK
    /// </summary>
    public class AISDKExample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InputField inputField;
        [SerializeField] private Text responseText;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button streamButton;
        [SerializeField] private Dropdown providerDropdown;
        [SerializeField] private Dropdown agentDropdown;
        [SerializeField] private Toggle ttsToggle;
        [SerializeField] private Slider volumeSlider;
        
        [Header("Settings")]
        [SerializeField] private bool autoRunOnStart = false;
        [SerializeField] private string testMessage = "Hello, what time is it?";
        
        private bool isProcessing = false;

        void Start()
        {
            SetupUI();
            
            if (autoRunOnStart)
            {
                StartCoroutine(RunExample());
            }
        }

        void SetupUI()
        {
            // Setup provider dropdown
            providerDropdown.ClearOptions();
            providerDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "OpenAI",
                "Gemini", 
                "DeepSeek",
                "Anthropic",
                "Cohere"
            });

            // Setup agent dropdown
            agentDropdown.ClearOptions();
            agentDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Assistant",
                "Creative",
                "Technical", 
                "Analytical",
                "Conversational"
            });

            // Setup buttons
            sendButton.onClick.AddListener(OnSendButtonClicked);
            streamButton.onClick.AddListener(OnStreamButtonClicked);
            
            // Setup volume slider
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            volumeSlider.value = 1.0f;

            // Subscribe to events
            AISDKCore.OnAIResponse += OnAIResponse;
            AISDKCore.OnAudioGenerated += OnAudioGenerated;
            AISDKCore.OnError += OnError;
            AISDKCore.OnSpeechStarted += OnSpeechStarted;
            AISDKCore.OnSpeechFinished += OnSpeechFinished;
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            AISDKCore.OnAIResponse -= OnAIResponse;
            AISDKCore.OnAudioGenerated -= OnAudioGenerated;
            AISDKCore.OnError -= OnError;
            AISDKCore.OnSpeechStarted -= OnSpeechStarted;
            AISDKCore.OnSpeechFinished -= OnSpeechFinished;
        }

        private System.Collections.IEnumerator RunExample()
        {
            // Wait for SDK to initialize
            yield return new WaitForSeconds(1f);

            if (!AISDKCore.Instance.IsInitialized)
            {
                Debug.LogError("AISDK not initialized!");
                yield break;
            }

            Debug.Log("Running AI SDK example...");

            // Example 1: Basic chat
            Debug.Log("Example 1: Basic chat");
            AISDKCore.Instance.SendMessage("Hello, who are you?", AgentType.Assistant, ttsToggle.isOn);
            yield return new WaitForSeconds(2f);

            // Example 2: Time query with extension
            Debug.Log("Example 2: Time query with extension");
            AISDKCore.Instance.SendMessage("What time is it?", AgentType.Assistant, ttsToggle.isOn);
            yield return new WaitForSeconds(2f);

            // Example 3: Creative response
            Debug.Log("Example 3: Creative response");
            AISDKCore.Instance.SendMessage("Tell me a short story", AgentType.Creative, ttsToggle.isOn);
            yield return new WaitForSeconds(2f);

            // Example 4: Technical question
            Debug.Log("Example 4: Technical question");
            AISDKCore.Instance.SendMessage("Explain how Unity coroutines work", AgentType.Technical, ttsToggle.isOn);
            yield return new WaitForSeconds(2f);

            Debug.Log("AI SDK example completed!");
        }

        private void OnSendButtonClicked()
        {
            if (isProcessing) return;

            string message = inputField.text;
            if (string.IsNullOrEmpty(message))
            {
                message = testMessage;
            }

            AgentType agentType = (AgentType)agentDropdown.value;
            bool generateSpeech = ttsToggle.isOn;

            Debug.Log($"Sending message: {message} to {agentType} agent");
            AISDKCore.Instance.SendMessage(message, agentType, generateSpeech);
        }

        private void OnStreamButtonClicked()
        {
            if (isProcessing) return;

            string message = inputField.text;
            if (string.IsNullOrEmpty(message))
            {
                message = testMessage;
            }

            AgentType agentType = (AgentType)agentDropdown.value;

            Debug.Log($"Streaming message: {message} to {agentType} agent");
            AISDKCore.Instance.StreamMessage(message, agentType, OnStreamChunk);
        }

        private void OnStreamChunk(string chunk, bool isComplete)
        {
            if (!string.IsNullOrEmpty(chunk))
            {
                responseText.text += chunk;
            }

            if (isComplete)
            {
                Debug.Log("Streaming completed");
                isProcessing = false;
            }
        }

        private void OnVolumeChanged(float volume)
        {
            AISDKCore.Instance.SetVolume(volume);
            Debug.Log($"Volume set to: {volume}");
        }

        private void OnAIResponse(string response)
        {
            Debug.Log($"AI Response: {response}");
            responseText.text = response;
            isProcessing = false;
        }

        private void OnAudioGenerated(AudioClip clip)
        {
            Debug.Log($"Audio generated: {clip.name} ({clip.length}s)");
        }

        private void OnError(string error)
        {
            Debug.LogError($"AI SDK Error: {error}");
            responseText.text = $"Error: {error}";
            isProcessing = false;
        }

        private void OnSpeechStarted()
        {
            Debug.Log("Speech started");
        }

        private void OnSpeechFinished()
        {
            Debug.Log("Speech finished");
        }

        // Public methods for external access
        public void SendTestMessage()
        {
            OnSendButtonClicked();
        }

        public void SwitchProvider(int providerIndex)
        {
            AIProviderType providerType = (AIProviderType)providerIndex;
            AISDKCore.Instance.SetProvider(providerType);
            Debug.Log($"Switched to provider: {providerType}");
        }

        public void StopAudio()
        {
            AISDKCore.Instance.StopAudio();
        }

        public void SetApiKey(string apiKey)
        {
            AISDKCore.Instance.SetApiKey(apiKey);
        }

        public void AddExtension(IAgentExtension extension)
        {
            AISDKCore.Instance.AddExtension(extension);
        }

        public void RemoveExtension(IAgentExtension extension)
        {
            AISDKCore.Instance.RemoveExtension(extension);
        }
    }
}
