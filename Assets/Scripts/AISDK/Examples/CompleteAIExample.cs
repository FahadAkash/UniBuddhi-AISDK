using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniBuddhi.Core;
using UniBuddhi.Core.Models;

public class CompleteAIExample : MonoBehaviour
{
    [Header("UI References")]
    public InputField messageInput;
    public Text responseText;
    public Button sendButton;
    public Button streamButton;
    public Dropdown agentDropdown;
    public Dropdown providerDropdown;
    public Toggle ttsToggle;
    public Slider volumeSlider;
    
    [Header("API Configuration")]
    public string openAIApiKey = "your-openai-key-here";
    public string geminiApiKey = "your-gemini-key-here";
    public string elevenLabsApiKey = "your-elevenlabs-key-here";
    
    private AISDKCore aiSDK;
    private bool isStreaming = false;
    
    void Start()
    {
        // Get AI SDK reference
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
        
        // Setup UI
        SetupUI();
        
        // Configure API keys
        ConfigureAPIKeys();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from static events
        AISDKCore.OnAIResponse -= HandleAIResponse;
        AISDKCore.OnError -= HandleError;
        AISDKCore.OnAudioGenerated -= HandleAudioGenerated;
    }
    
    void SetupUI()
    {
        // Setup buttons
        sendButton.onClick.AddListener(SendMessage);
        streamButton.onClick.AddListener(StartStreaming);
        
        // Setup dropdowns
        agentDropdown.onValueChanged.AddListener(OnAgentChanged);
        providerDropdown.onValueChanged.AddListener(OnProviderChanged);
        
        // Setup volume slider
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        volumeSlider.value = 1.0f;
        
        // Initialize response text
        responseText.text = "AI SDK Ready! Type a message and click Send.";
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
            // Add other providers as needed
        }
        
        // Set TTS API key if using ElevenLabs
        // Note: TTS configuration should be done through the TTS provider directly
        // or through the SDK's TTS configuration methods
    }
    
    public void SendMessage()
    {
        if (string.IsNullOrEmpty(messageInput.text)) return;
        
        string message = messageInput.text;
        AgentType selectedAgent = (AgentType)agentDropdown.value;
        bool useTTS = ttsToggle.isOn;
        
        responseText.text = "Thinking...";
        
        // Send message to AI
        aiSDK.SendMessage(message, selectedAgent, useTTS);
        
        // Clear input
        messageInput.text = "";
    }
    
    public void StartStreaming()
    {
        if (string.IsNullOrEmpty(messageInput.text) || isStreaming) return;
        
        string message = messageInput.text;
        AgentType selectedAgent = (AgentType)agentDropdown.value;
        
        responseText.text = "";
        isStreaming = true;
        streamButton.interactable = false;
        
        // Start streaming response
        aiSDK.StreamMessage(message, selectedAgent, OnStreamChunk);
        
        // Clear input
        messageInput.text = "";
    }
    
    private void OnStreamChunk(string chunk, bool isComplete)
    {
        if (!isComplete)
        {
            responseText.text += chunk;
        }
        else
        {
            responseText.text += "\n\n[Streaming Complete]";
            isStreaming = false;
            streamButton.interactable = true;
        }
    }
    
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
    
    private void OnAgentChanged(int agentIndex)
    {
        AgentType newAgent = (AgentType)agentIndex;
        Debug.Log($"Switched to agent: {newAgent}");
    }
    
    private void OnProviderChanged(int providerIndex)
    {
        AIProviderType newProvider = (AIProviderType)providerIndex;
        aiSDK.SetProvider(newProvider);
        ConfigureAPIKeys();
        Debug.Log($"Switched to provider: {newProvider}");
    }
    
    private void OnVolumeChanged(float volume)
    {
        aiSDK.SetVolume(volume);
    }
    
    public void StopAudio()
    {
        aiSDK.StopAudio();
    }
    
    // Test different agent types
    public void TestDifferentAgents()
    {
        StartCoroutine(TestAgentsCoroutine());
    }
    
    private IEnumerator TestAgentsCoroutine()
    {
        string[] testMessages = {
            "Explain quantum physics",
            "Write a creative story",
            "Help me debug my code",
            "Analyze this data trend",
            "Let's have a casual chat"
        };
        
        AgentType[] agents = {
            AgentType.Assistant,
            AgentType.Creative,
            AgentType.Technical,
            AgentType.Analytical,
            AgentType.Conversational
        };
        
        for (int i = 0; i < testMessages.Length; i++)
        {
            responseText.text = $"Testing {agents[i]}...";
            aiSDK.SendMessage(testMessages[i], agents[i], false);
            yield return new WaitForSeconds(3f);
        }
    }
}
