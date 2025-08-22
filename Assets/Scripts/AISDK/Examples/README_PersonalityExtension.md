# 🎭 Personality Agent Extension - User Guide

## Overview

The **Personality Agent Extension** is a powerful addition to your AI SDK that allows you to create and switch between different AI personalities, each with their own unique system prompts, response styles, and behavioral characteristics.

## ✨ Features

- **5 Built-in Personalities**: Friendly Companion, Wise Mentor, Playful Jester, Curious Explorer, and Gentle Healer
- **Custom Personality Creation**: Create your own unique AI personalities
- **Dynamic System Prompts**: Override the AI's default behavior with personality-specific instructions
- **Response Style Modification**: Automatically adjust AI responses to match personality characteristics
- **Topic Preferences**: Define what topics each personality loves or avoids
- **Real-time Switching**: Change personalities on the fly during conversations

## 🚀 Quick Start

### 1. Setup the Extension

1. **Add to Scene**: Add the `PersonalityAgentExtension` component to any GameObject in your scene
2. **Auto-Discovery**: The extension will automatically be discovered by the AI SDK
3. **Configure Settings**: Adjust the extension settings in the Inspector

### 2. Basic Usage

```csharp
// Get the extension
var personalityExtension = FindFirstObjectByType<PersonalityAgentExtension>();

// Switch to different personalities
personalityExtension.SetPersonality(PersonalityType.FriendlyCompanion);
personalityExtension.SetPersonality(PersonalityType.WiseMentor);
personalityExtension.SetPersonality(PersonalityType.PlayfulJester);

// Send a message - the AI will respond with the selected personality
aiSDK.SendMessage("Hello! How are you?", AgentType.Assistant, false);
```

## 🎯 Built-in Personalities

### 🤗 Friendly Companion
- **Style**: Warm, encouraging, optimistic
- **Best For**: Daily conversations, motivation, emotional support
- **Example Response**: "Oh no, I'm so sorry you're having a tough day! 😊 You know what though? You're incredibly strong and you've gotten through difficult days before - you've got this! 💪"

### 🧙 Wise Mentor
- **Style**: Thoughtful, philosophical, story-driven
- **Best For**: Life advice, deep discussions, personal growth
- **Example Response**: "Ah... the storms of life visit us all, my friend... Consider this: even the mightiest oak tree bends in the wind but does not break... What do you think this difficult day might be teaching you about your own resilience...?"

### 🎪 Playful Jester
- **Style**: Humorous, witty, entertaining
- **Best For**: Fun conversations, jokes, creative projects
- **Example Response**: "Oh dear! Sounds like your day is having a case of the Mondays - even if it's Tuesday! *winks playfully* You know what I call a bad day? A good story in disguise! 🎭"

### 🔍 Curious Explorer
- **Style**: Inquisitive, wonder-filled, exploratory
- **Best For**: Learning, discovery, scientific discussions
- **Example Response**: "That's fascinating! I wonder what other mysteries might be hiding in that topic? What if we explored it from a completely different angle? Have you ever thought about how this connects to other areas of knowledge?"

### 💚 Gentle Healer
- **Style**: Compassionate, soothing, nurturing
- **Best For**: Emotional support, stress relief, healing conversations
- **Example Response**: "I can feel how much this is weighing on you... Your feelings are completely valid, and it's okay to take time to process them. Remember, healing isn't always linear - take your time with this. 💙"

## 🛠️ Creating Custom Personalities

### Method 1: Through Code

```csharp
var customPersonality = new PersonalityAgent
{
    Type = PersonalityType.Custom,
    Name = "Space Explorer",
    SystemPrompt = @"You are an enthusiastic space explorer from the year 3024. 
    You've traveled across galaxies and discovered countless alien civilizations. 
    You speak with wonder about space, use futuristic terminology, and always 
    relate conversations back to cosmic exploration.",
    
    PersonalityTraits = new List<string> 
    { 
        "adventurous", "scientific", "optimistic", "cosmic", "enthusiastic" 
    },
    
    ResponseStyle = "Use space terminology, express wonder about the cosmos, " +
                   "mention your space adventures, use phrases like 'stellar!', " +
                   "'cosmic coincidence!', 'intergalactic wisdom'",
    
    PreferredTopics = new List<string> 
    { 
        "space exploration", "alien life", "cosmology", "future technology", 
        "interstellar travel" 
    },
    
    AvoidedTopics = new List<string> 
    { 
        "anti-science sentiment", "earth-bound limitations" 
    }
};

// Add to extension
personalityExtension.AddCustomPersonality(customPersonality);

// Switch to it
personalityExtension.SetPersonality(PersonalityType.Custom);
```

### Method 2: Through the UI

Use the `PersonalityManagerUI` component which provides:
- Dropdown to select personalities
- Info panel showing personality details
- Chat interface with personality-aware responses
- Custom personality creation panel

## ⚙️ Configuration Options

### Extension Settings

- **Override System Prompt**: Whether to replace the AI's default system prompt
- **Add Personality Context**: Whether to inject personality information into conversations
- **Use Personality Specific Responses**: Whether to modify AI responses based on personality
- **Show Personality in Response**: Whether to label responses with personality name

### Personality Properties

- **System Prompt**: The core instruction that defines the personality
- **Personality Traits**: Keywords that describe the character
- **Response Style**: Specific instructions for how to communicate
- **Preferred Topics**: Subjects the personality loves to discuss
- **Avoided Topics**: Subjects the personality prefers to avoid
- **Creativity Level**: How creative/imaginative the personality should be
- **Response Enthusiasm**: How energetic/enthusiastic responses should be

## 🔄 How It Works

### 1. Preprocessing Phase
When a user sends a message, the extension:
- Identifies the current active personality
- Builds a comprehensive context including:
  - Personality system prompt
  - Traits and response style
  - Topic preferences
  - Behavioral instructions

### 2. AI Processing
The AI receives the enhanced context and responds according to the personality's characteristics

### 3. Postprocessing Phase
The extension can modify the AI's response to:
- Add personality-specific touches
- Ensure consistency with the character
- Apply response style modifications

## 📱 UI Integration

### PersonalityManagerUI Component

This component provides a complete UI for managing personalities:

```csharp
// Add to your scene
var personalityUI = gameObject.AddComponent<PersonalityManagerUI>();

// Configure UI references in the Inspector
personalityUI.personalityDropdown = yourDropdown;
personalityUI.personalityInfoText = yourInfoText;
personalityUI.chatOutput = yourChatText;
// ... etc
```

### Key UI Features

- **Personality Selection**: Dropdown to choose from available personalities
- **Info Display**: Shows detailed information about selected personality
- **Chat Interface**: Real-time chat with personality-aware responses
- **Creation Panel**: Build custom personalities through the UI
- **Settings Toggles**: Configure extension behavior

## 🎮 Example Scenarios

### Gaming Assistant
Create a personality that:
- Uses gaming terminology (GG, level up, respawn)
- References popular games
- Provides gaming tips and strategies
- Maintains competitive but friendly attitude

### Educational Tutor
Create a personality that:
- Asks probing questions
- Provides step-by-step explanations
- Encourages critical thinking
- Adapts to different learning styles

### Creative Writing Partner
Create a personality that:
- Suggests creative ideas
- Helps with story development
- Provides constructive feedback
- Maintains artistic enthusiasm

## 🔧 Advanced Usage

### Dynamic Personality Switching

```csharp
// Switch personalities based on context
if (userMessage.Contains("joke") || userMessage.Contains("funny"))
{
    personalityExtension.SetPersonality(PersonalityType.PlayfulJester);
}
else if (userMessage.Contains("advice") || userMessage.Contains("help"))
{
    personalityExtension.SetPersonality(PersonalityType.WiseMentor);
}
else if (userMessage.Contains("science") || userMessage.Contains("discover"))
{
    personalityExtension.SetPersonality(PersonalityType.CuriousExplorer);
}
```

### Personality Chaining

```csharp
// Create a conversation flow with different personalities
var conversationFlow = new List<PersonalityType>
{
    PersonalityType.FriendlyCompanion,    // Start friendly
    PersonalityType.CuriousExplorer,      // Ask questions
    PersonalityType.WiseMentor,           // Provide insights
    PersonalityType.GentleHealer,         // Offer comfort
    PersonalityType.PlayfulJester         // End with humor
};

int currentPersonalityIndex = 0;
personalityExtension.SetPersonality(conversationFlow[currentPersonalityIndex]);
```

### Event Handling

```csharp
// Subscribe to personality change events
personalityExtension.OnPersonalityChanged += (newPersonality) =>
{
    Debug.Log($"Switched to: {newPersonality}");
    // Update UI, play sound effects, etc.
};

// Subscribe to context addition events
personalityExtension.OnPersonalityContextAdded += (context) =>
{
    Debug.Log($"Added context: {context.Substring(0, 100)}...");
    // Log or display the context being added
};
```

## 🚨 Troubleshooting

### Common Issues

1. **Personality Not Switching**
   - Check if the extension is properly initialized
   - Verify the personality exists in the database
   - Ensure the extension is added to the AI SDK

2. **Responses Not Following Personality**
   - Check if "Override System Prompt" is enabled
   - Verify "Add Personality Context" is enabled
   - Ensure the system prompt is clear and specific

3. **Extension Not Found**
   - Make sure the extension component is in the scene
   - Check if it's properly added to the AI SDK
   - Verify the component is enabled

### Debug Tips

- Enable debug logging in the extension
- Check the console for personality context being added
- Verify event subscriptions are working
- Test with simple, clear system prompts first

## 🎯 Best Practices

### Writing Effective System Prompts

1. **Be Specific**: Clearly define the personality's role and behavior
2. **Use Examples**: Include specific phrases or responses they should use
3. **Set Boundaries**: Define what topics they should avoid
4. **Maintain Consistency**: Ensure the prompt creates a coherent character

### Personality Design

1. **Start Simple**: Begin with basic traits and expand
2. **Test Thoroughly**: Try different conversation topics
3. **Iterate**: Refine based on actual AI responses
4. **Balance**: Mix positive and neutral traits for realism

### Performance Considerations

1. **Limit Personalities**: Too many can impact performance
2. **Optimize Prompts**: Keep system prompts concise but effective
3. **Cache Results**: Store frequently used personality configurations
4. **Monitor Memory**: Watch for memory leaks with dynamic creation

## 🔮 Future Enhancements

Potential improvements for the extension:
- **Personality Blending**: Mix multiple personalities
- **Context Awareness**: Adapt based on conversation history
- **Emotional States**: Dynamic personality variations
- **Learning**: Personalities that adapt to user preferences
- **Voice Integration**: Different voices for different personalities

## 📚 Additional Resources

- Check the `PersonalityExampleUsage.cs` for code examples
- Use `PersonalityManagerUI.cs` for UI integration
- Review the main AI SDK documentation for core functionality
- Explore the extension system for creating other types of extensions

---

**Happy Personality Crafting! 🎭✨**
