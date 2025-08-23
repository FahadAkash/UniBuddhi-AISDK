#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UniBuddhi.Core;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Editor
{
    public class AgentCreationWizard : EditorWindow
    {
        #region Constants
        private const string WindowTitle = "🧙‍♂️ Agent Creation Wizard";
        private const float WizardWidth = 600f;
        private const float WizardHeight = 700f;
        #endregion
        
        #region Private Fields
        private int _currentStep = 0;
        private readonly string[] _stepTitles = {
            "🎯 Basic Info",
            "🧠 Personality",
            "🔌 Extensions",
            "⚙️ Advanced",
            "✅ Review"
        };
        
        // Agent Data
        private string _agentName = "MyAgent";
        private AgentType _agentType = AgentType.Assistant;
        private string _description = "";
        private string _systemPrompt = "You are a helpful AI assistant.";
        private float _temperature = 0.7f;
        private int _maxTokens = 1000;
        private List<string> _selectedExtensions = new List<string>();
        private Dictionary<string, float> _personalityTraits = new Dictionary<string, float>();
        
        // UI
        private Vector2 _scrollPos;
        private GUIStyle _headerStyle;
        private GUIStyle _stepStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _titleStyle;
        #endregion
        
        #region Public Methods
        public static void ShowWizard()
        {
            var window = GetWindow<AgentCreationWizard>(true, WindowTitle, true);
            window.minSize = new Vector2(WizardWidth, WizardHeight);
            window.maxSize = new Vector2(WizardWidth, WizardHeight);
            window.Show();
        }
        #endregion
        
        #region Unity Events
        private void OnEnable()
        {
            InitializeStyles();
            InitializePersonalityTraits();
        }
        
        private void OnGUI()
        {
            if (_headerStyle == null) InitializeStyles();
            
            DrawHeader();
            DrawStepIndicator();
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            DrawCurrentStep();
            EditorGUILayout.EndScrollView();
            
            DrawNavigation();
        }
        #endregion
        
        #region Style Initialization
        private void InitializeStyles()
        {
            _headerStyle = new GUIStyle(EditorStyles.toolbar)
            {
                fixedHeight = 50f,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            
            _stepStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 30f,
                fontSize = 10
            };
            
            _cardStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(15, 15, 15, 15),
                margin = new RectOffset(10, 10, 5, 5)
            };
            
            _titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                richText = true
            };
        }
        
        private void InitializePersonalityTraits()
        {
            _personalityTraits = new Dictionary<string, float>
            {
                { "Creativity", 0.5f },
                { "Analytical", 0.5f },
                { "Friendliness", 0.8f },
                { "Formality", 0.3f },
                { "Humor", 0.4f },
                { "Empathy", 0.7f }
            };
        }
        #endregion
        
        #region Drawing Methods
        private void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.15f, 0.2f));
            GUI.Label(rect, "🧙‍♂️ Agent Creation Wizard", _headerStyle);
        }
        
        private void DrawStepIndicator()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            
            for (int i = 0; i < _stepTitles.Length; i++)
            {
                var isActive = i == _currentStep;
                var isCompleted = i < _currentStep;
                
                var style = new GUIStyle(_stepStyle);
                if (isActive)
                {
                    style.normal.background = style.active.background;
                }
                else if (isCompleted)
                {
                    style.normal.textColor = Color.green;
                }
                
                if (GUILayout.Button(_stepTitles[i], style))
                {
                    _currentStep = i;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
        
        private void DrawCurrentStep()
        {
            switch (_currentStep)
            {
                case 0: DrawBasicInfoStep(); break;
                case 1: DrawPersonalityStep(); break;
                case 2: DrawExtensionsStep(); break;
                case 3: DrawAdvancedStep(); break;
                case 4: DrawReviewStep(); break;
            }
        }
        
        private void DrawBasicInfoStep()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("🎯 <b>Basic Agent Information</b>", _titleStyle);
            GUILayout.Space(10);
            
            _agentName = EditorGUILayout.TextField("Agent Name", _agentName);
            _agentType = (AgentType)EditorGUILayout.EnumPopup("Agent Type", _agentType);
            
            GUILayout.Label("Description:", EditorStyles.boldLabel);
            _description = EditorGUILayout.TextArea(_description, GUILayout.Height(60));
            
            // Agent Type Description
            GUILayout.Space(10);
            DrawAgentTypeInfo();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPersonalityStep()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("🧠 <b>Agent Personality</b>", _titleStyle);
            GUILayout.Space(10);
            
            GUILayout.Label("System Prompt:", EditorStyles.boldLabel);
            _systemPrompt = EditorGUILayout.TextArea(_systemPrompt, GUILayout.Height(100));
            
            GUILayout.Space(15);
            GUILayout.Label("Personality Traits:", EditorStyles.boldLabel);
            
            var traits = new List<string>(_personalityTraits.Keys);
            foreach (var trait in traits)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(trait, GUILayout.Width(100));
                _personalityTraits[trait] = EditorGUILayout.Slider(_personalityTraits[trait], 0f, 1f);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawExtensionsStep()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("🔌 <b>Available Extensions</b>", _titleStyle);
            GUILayout.Space(10);
            
            var availableExtensions = new Dictionary<string, string>
            {
                { "PersonalityExtension", "Adds personality-based behavior modification" },
                { "CurrentTimeExtension", "Provides current time and date context" },
                { "WeatherExtension", "Supplies weather information" },
                { "CalculatorExtension", "Enables mathematical calculations" },
                { "KnowledgeSearchExtension", "Adds knowledge search capabilities" }
            };
            
            foreach (var ext in availableExtensions)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                var isSelected = _selectedExtensions.Contains(ext.Key);
                var newSelected = EditorGUILayout.Toggle($"🔌 {ext.Key}", isSelected);
                
                if (newSelected != isSelected)
                {
                    if (newSelected)
                        _selectedExtensions.Add(ext.Key);
                    else
                        _selectedExtensions.Remove(ext.Key);
                }
                
                if (isSelected)
                {
                    EditorGUILayout.LabelField(ext.Value, EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAdvancedStep()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("⚙️ <b>Advanced Configuration</b>", _titleStyle);
            GUILayout.Space(10);
            
            _temperature = EditorGUILayout.Slider("Temperature (Creativity)", _temperature, 0f, 2f);
            EditorGUILayout.HelpBox("Higher values make output more random, lower values more focused.", MessageType.Info);
            
            GUILayout.Space(10);
            _maxTokens = EditorGUILayout.IntSlider("Max Tokens", _maxTokens, 100, 4000);
            EditorGUILayout.HelpBox("Maximum number of tokens in the response.", MessageType.Info);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawReviewStep()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("✅ <b>Review & Create</b>", _titleStyle);
            GUILayout.Space(10);
            
            // Summary
            GUILayout.Label($"<b>Agent Name:</b> {_agentName}", new GUIStyle(EditorStyles.label) { richText = true });
            GUILayout.Label($"<b>Type:</b> {_agentType}", new GUIStyle(EditorStyles.label) { richText = true });
            GUILayout.Label($"<b>Temperature:</b> {_temperature:F2}", new GUIStyle(EditorStyles.label) { richText = true });
            GUILayout.Label($"<b>Max Tokens:</b> {_maxTokens}", new GUIStyle(EditorStyles.label) { richText = true });
            GUILayout.Label($"<b>Extensions:</b> {_selectedExtensions.Count}", new GUIStyle(EditorStyles.label) { richText = true });
            
            if (_selectedExtensions.Count > 0)
            {
                foreach (var ext in _selectedExtensions)
                {
                    GUILayout.Label($"  • {ext}", EditorStyles.miniLabel);
                }
            }
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("🚀 Create Agent", GUILayout.Height(40)))
            {
                CreateAgent();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAgentTypeInfo()
        {
            var descriptions = new Dictionary<AgentType, string>
            {
                { AgentType.Assistant, "General-purpose helper, balanced and versatile" },
                { AgentType.Creative, "Imaginative and expressive, great for storytelling" },
                { AgentType.Technical, "Precise and analytical, perfect for technical tasks" },
                { AgentType.Analytical, "Data-driven and logical, excellent for analysis" },
                { AgentType.Conversational, "Friendly and engaging, ideal for chat interactions" }
            };
            
            if (descriptions.ContainsKey(_agentType))
            {
                EditorGUILayout.HelpBox(descriptions[_agentType], MessageType.Info);
            }
        }
        
        private void DrawNavigation()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            
            // Back Button
            GUI.enabled = _currentStep > 0;
            if (GUILayout.Button("◀ Back", GUILayout.Height(30)))
            {
                _currentStep--;
            }
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            
            // Next/Finish Button
            var isLastStep = _currentStep == _stepTitles.Length - 1;
            var buttonText = isLastStep ? "🏁 Finish" : "Next ▶";
            
            if (GUILayout.Button(buttonText, GUILayout.Height(30)))
            {
                if (isLastStep)
                {
                    CreateAgent();
                }
                else
                {
                    _currentStep++;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
        #endregion
        
        #region Agent Creation
        private void CreateAgent()
        {
            if (string.IsNullOrEmpty(_agentName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter an agent name!", "OK");
                return;
            }
            
            var personality = new AgentPersonality(_agentName)
            {
                SystemPrompt = _systemPrompt,
                Temperature = _temperature,
                MaxTokens = _maxTokens,
                Traits = _personalityTraits
            };
            
            if (AISDKCore.Instance != null)
            {
                var agent = AISDKCore.Instance.CreateAgent(
                    _agentName,
                    _agentType,
                    personality,
                    _selectedExtensions.ToArray()
                );
                
                if (agent != null)
                {
                    EditorUtility.DisplayDialog("Success!", 
                        $"Agent '{_agentName}' has been created successfully!\n\nYou can now use it in your scenes.",
                        "Awesome!");
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to create agent. Please check the console for details.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", 
                    "AISDKCore not found in the scene!\n\nPlease add AISDKCore component to a GameObject first.",
                    "OK");
            }
        }
        #endregion
    }
}
#endif