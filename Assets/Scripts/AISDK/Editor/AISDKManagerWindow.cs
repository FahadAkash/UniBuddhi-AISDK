#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniBuddhi.Core;
using UniBuddhi.Core.Models;
using UniBuddhi.Core.Interfaces;

namespace UniBuddhi.Editor
{
    public class AISDKManagerWindow : EditorWindow
    {
        #region Constants & Styles
        private const string WindowTitle = "🧠 AI SDK Manager";
        private const float TabHeight = 35f;
        private const float HeaderHeight = 50f;
        private const float SidebarWidth = 250f;
        
        // Modern Color Palette
        private static readonly Color PrimaryColor = new Color(0.13f, 0.59f, 0.95f, 1f);
        private static readonly Color SecondaryColor = new Color(0.12f, 0.14f, 0.17f, 1f);
        private static readonly Color AccentColor = new Color(0.26f, 0.84f, 0.41f, 1f);
        #endregion

        #region Private Fields
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "🤖 Agents", "🔌 Extensions", "⚙️ Providers", "📊 Analytics" };
        
        private Vector2 _mainScrollPos;
        private Vector2 _sidebarScrollPos;
        
        // Agent Creation
        private string _newAgentName = "MyAgent";
        private AgentType _newAgentType = AgentType.Assistant;
        private string _newAgentPrompt = "You are a helpful AI assistant.";
        private float _newAgentTemperature = 0.7f;
        private int _newAgentMaxTokens = 1000;
        private List<string> _selectedExtensions = new List<string>();
        
        // Extension Creation
        private string _newExtensionName = "MyExtension";
        private string _newExtensionDescription = "Custom extension";
        private ExtensionType _newExtensionType = ExtensionType.Simple;
        
        // Styles
        private GUIStyle _headerStyle;
        private GUIStyle _tabStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        
        // Data
        private List<AgentInfo> _agentList = new List<AgentInfo>();
        private List<ExtensionInfo> _extensionList = new List<ExtensionInfo>();
        #endregion

        #region Menu Items
        [MenuItem("Tools/UniBuddhi AI SDK/🧠 AI SDK Manager", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<AISDKManagerWindow>(WindowTitle);
            window.minSize = new Vector2(900, 600);
            window.Show();
        }
        #endregion

        #region Unity Events
        private void OnEnable()
        {
            RefreshData();
        }

        private void OnGUI()
        {
            try
            {
                if (_headerStyle == null) 
                    InitializeStyles();
                
                DrawHeader();
                DrawTabs();
                
                EditorGUILayout.BeginHorizontal();
                try
                {
                    DrawSidebar();
                    DrawMainContent();
                }
                finally
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
            catch (System.Exception e)
            {
                EditorGUILayout.HelpBox($"GUI Error: {e.Message}", MessageType.Error);
                if (GUILayout.Button("Reset Window"))
                {
                    _headerStyle = null;
                    Repaint();
                }
            }
        }
        #endregion

        #region Style Initialization
        private void InitializeStyles()
        {
            _headerStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            
            _tabStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                fixedHeight = TabHeight
            };
            
            _cardStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(5, 5, 3, 3)
            };
            
            _titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                richText = true
            };
            
            _labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                richText = true,
                wordWrap = true
            };
            
            _buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 10,
                fixedHeight = 25,
                fontStyle = FontStyle.Normal
            };
        }
        #endregion

        #region Header & Tabs
        private void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, HeaderHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, SecondaryColor);
            
            GUILayout.BeginArea(rect);
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            
            GUILayout.Label("🧠 UniBuddhi AI SDK Manager", _headerStyle);
            GUILayout.FlexibleSpace();
            
            // Status
            var isReady = AISDKCore.Instance != null && AISDKCore.Instance.IsInitialized;
            var statusText = isReady ? "✓ Ready" : "⚠ Not Ready";
            var statusColor = isReady ? Color.green : Color.red;
            
            var statusStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = statusColor },
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            
            GUILayout.Label(statusText, statusStyle);
            GUILayout.Space(15);
            
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();
            
            for (int i = 0; i < _tabNames.Length; i++)
            {
                var isSelected = _selectedTab == i;
                var style = new GUIStyle(_tabStyle);
                
                if (isSelected)
                {
                    style.normal.background = style.active.background;
                    style.fontStyle = FontStyle.Bold;
                }
                
                if (GUILayout.Button(_tabNames[i], style))
                {
                    if (_selectedTab != i)
                    {
                        _selectedTab = i;
                        RefreshData();
                    }
                }
            }
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("🔄 Refresh", EditorStyles.miniButton))
            {
                RefreshData();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Sidebar
        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(SidebarWidth));
            
            GUILayout.Space(5);
            _sidebarScrollPos = EditorGUILayout.BeginScrollView(_sidebarScrollPos);
            
            try
            {
                switch (_selectedTab)
                {
                    case 0: DrawAgentSidebar(); break;
                    case 1: DrawExtensionSidebar(); break;
                    case 2: DrawProviderSidebar(); break;
                    case 3: DrawAnalyticsSidebar(); break;
                }
            }
            catch (System.Exception e)
            {
                EditorGUILayout.HelpBox($"Sidebar Error: {e.Message}", MessageType.Error);
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawAgentSidebar()
        {
            GUILayout.Label("🤖 Agent Actions", _titleStyle);
            GUILayout.Space(5);
            
            if (GUILayout.Button("➕ Create New Agent", _buttonStyle))
            {
                CreateNewAgent();
            }
            
            if (GUILayout.Button("📂 Load Template", _buttonStyle))
            {
                LoadAgentTemplate();
            }
            
            if (GUILayout.Button("💾 Save Preset", _buttonStyle))
            {
                SaveAgentPreset();
            }
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("📋 Quick Stats", EditorStyles.boldLabel);
            GUILayout.Label($"Total Agents: {_agentList.Count}", _labelStyle);
            GUILayout.Label($"Active: {_agentList.Count(a => a.IsActive)}", _labelStyle);
            EditorGUILayout.EndVertical();
        }
        
        private void DrawExtensionSidebar()
        {
            GUILayout.Label("🔌 Extension Actions", _titleStyle);
            GUILayout.Space(5);
            
            if (GUILayout.Button("➕ Create Extension", _buttonStyle))
            {
                CreateNewExtension();
            }
            
            if (GUILayout.Button("📜 Generate Script", _buttonStyle))
            {
                GenerateExtensionScript();
            }
            
            if (GUILayout.Button("🔄 Refresh Extensions", _buttonStyle))
            {
                RefreshExtensions();
            }
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("📊 Extension Stats", EditorStyles.boldLabel);
            GUILayout.Label($"Total Extensions: {_extensionList.Count}", _labelStyle);
            EditorGUILayout.EndVertical();
        }
        
        private void DrawProviderSidebar()
        {
            GUILayout.Label("⚙️ Provider Settings", _titleStyle);
            GUILayout.Space(5);
            
            if (GUILayout.Button("🔑 Configure APIs", _buttonStyle))
            {
                ConfigureAPIKeys();
            }
            
            if (GUILayout.Button("🧪 Test Connections", _buttonStyle))
            {
                TestProviderConnections();
            }
            
            if (GUILayout.Button("📋 View Status", _buttonStyle))
            {
                ShowProviderStatus();
            }
        }
        
        private void DrawAnalyticsSidebar()
        {
            GUILayout.Label("📊 Analytics Tools", _titleStyle);
            GUILayout.Space(5);
            
            if (GUILayout.Button("📈 Usage Report", _buttonStyle))
            {
                GenerateUsageReport();
            }
            
            if (GUILayout.Button("💰 Cost Analysis", _buttonStyle))
            {
                ShowCostAnalysis();
            }
            
            if (GUILayout.Button("🔄 Clear Data", _buttonStyle))
            {
                ClearAnalyticsData();
            }
        }
        #endregion

        #region Main Content Areas
        private void DrawMainContent()
        {
            EditorGUILayout.BeginVertical();
            
            GUILayout.Space(5);
            _mainScrollPos = EditorGUILayout.BeginScrollView(_mainScrollPos);
            
            try
            {
                switch (_selectedTab)
                {
                    case 0: DrawAgentManager(); break;
                    case 1: DrawExtensionManager(); break;
                    case 2: DrawProviderManager(); break;
                    case 3: DrawAnalytics(); break;
                }
            }
            catch (System.Exception e)
            {
                EditorGUILayout.HelpBox($"Content Error: {e.Message}", MessageType.Error);
                if (GUILayout.Button("Reset"))
                {
                    _headerStyle = null;
                    Repaint();
                }
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawAgentManager()
        {
            GUILayout.Label("🤖 Agent Management", _titleStyle);
            GUILayout.Space(8);
            
            // Agent Creation Form
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("➕ Create New Agent", _titleStyle);
            
            _newAgentName = EditorGUILayout.TextField("Agent Name", _newAgentName);
            _newAgentType = (AgentType)EditorGUILayout.EnumPopup("Agent Type", _newAgentType);
            
            GUILayout.Label("System Prompt:", EditorStyles.boldLabel);
            _newAgentPrompt = EditorGUILayout.TextArea(_newAgentPrompt, GUILayout.Height(60));
            
            _newAgentTemperature = EditorGUILayout.Slider("Temperature", _newAgentTemperature, 0f, 2f);
            _newAgentMaxTokens = EditorGUILayout.IntSlider("Max Tokens", _newAgentMaxTokens, 100, 4000);
            
            // Extension Selection
            GUILayout.Label("Extensions:", EditorStyles.boldLabel);
            DrawExtensionSelector();
            
            GUILayout.Space(5);
            if (GUILayout.Button("🚀 Create Agent", _buttonStyle))
            {
                CreateAgent();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Existing Agents List
            GUILayout.Label("📋 Existing Agents", _titleStyle);
            DrawAgentsList();
        }
        
        private void DrawExtensionManager()
        {
            GUILayout.Label("🔌 Extension Management", _titleStyle);
            GUILayout.Space(8);
            
            // Extension Creation Form
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("➕ Create New Extension", _titleStyle);
            
            _newExtensionName = EditorGUILayout.TextField("Extension Name", _newExtensionName);
            _newExtensionDescription = EditorGUILayout.TextField("Description", _newExtensionDescription);
            _newExtensionType = (ExtensionType)EditorGUILayout.EnumPopup("Extension Type", _newExtensionType);
            
            GUILayout.Space(5);
            if (GUILayout.Button("🚀 Create Extension", _buttonStyle))
            {
                CreateExtension();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Extension List
            GUILayout.Label("📋 Available Extensions", _titleStyle);
            DrawExtensionsList();
        }
        
        private void DrawProviderManager()
        {
            GUILayout.Label("⚙️ Provider Management", _titleStyle);
            GUILayout.Space(8);
            
            // Provider Configuration
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("🔧 Provider Configuration", _titleStyle);
            
            if (AISDKCore.Instance != null)
            {
                var currentProvider = AISDKCore.Instance.CurrentProvider;
                GUILayout.Label($"Current Provider: {currentProvider}", _labelStyle);
                
                GUILayout.Space(5);
                DrawProviderSettings();
            }
            else
            {
                EditorGUILayout.HelpBox("AISDKCore not found in scene. Please add it first.", MessageType.Warning);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAnalytics()
        {
            GUILayout.Label("📊 Analytics Dashboard", _titleStyle);
            GUILayout.Space(8);
            
            // Analytics Cards
            DrawAnalyticsCards();
        }
        #endregion

        #region Helper Methods
        private void RefreshData()
        {
            _agentList.Clear();
            _extensionList.Clear();
            
            if (AISDKCore.Instance != null)
            {
                var agents = AISDKCore.Instance.AllAgents;
                foreach (var agent in agents)
                {
                    _agentList.Add(new AgentInfo
                    {
                        Name = agent.Key,
                        Type = agent.Value.Type,
                        IsActive = true
                    });
                }
            }
        }

        private void CreateAgent()
        {
            if (string.IsNullOrEmpty(_newAgentName))
            {
                EditorUtility.DisplayDialog("Error", "Agent name cannot be empty!", "OK");
                return;
            }
            
            var personality = new AgentPersonality(_newAgentName)
            {
                SystemPrompt = _newAgentPrompt,
                Temperature = _newAgentTemperature,
                MaxTokens = _newAgentMaxTokens
            };
            
            if (AISDKCore.Instance != null)
            {
                var agent = AISDKCore.Instance.CreateAgent(
                    _newAgentName,
                    _newAgentType,
                    personality,
                    _selectedExtensions.ToArray()
                );
                
                if (agent != null)
                {
                    EditorUtility.DisplayDialog("Success", $"Agent '{_newAgentName}' created successfully!", "OK");
                    RefreshData();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "AISDKCore not found in scene!", "OK");
            }
        }
        
        private void CreateExtension()
        {
            if (string.IsNullOrEmpty(_newExtensionName))
            {
                EditorUtility.DisplayDialog("Error", "Extension name cannot be empty!", "OK");
                return;
            }
            
            GenerateExtensionScript();
        }
        
        private void GenerateExtensionScript()
        {
            var scriptContent = GenerateExtensionTemplate(_newExtensionName, _newExtensionDescription, _newExtensionType);
            var path = $"Assets/Scripts/AISDK/Core/Extensions/{_newExtensionName}.cs";
            
            File.WriteAllText(path, scriptContent);
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", $"Extension script '{_newExtensionName}.cs' created!", "OK");
        }
        
        private string GenerateExtensionTemplate(string name, string description, ExtensionType type)
        {
            return $@"using UnityEngine;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;
using UniBuddhi.Core.Extensions;

namespace UniBuddhi.Core.Extensions
{{
    public class {name} : BaseExtension
    {{
        public override string ExtensionName => ""{name}"";
        public override string Description => ""{description}"";
        
        public override bool CanPreprocess => true;
        public override bool CanPostprocess => true;
        
        public override string Preprocess(string input, IAgent agent)
        {{
            // Add your preprocessing logic here
            return input;
        }}
        
        public override string Postprocess(string output, IAgent agent)
        {{
            // Add your postprocessing logic here
            return output;
        }}
    }}
}}";
        }
        
        // Implementation methods
        private void CreateNewAgent() 
        {
            AgentCreationWizard.ShowWizard();
        }
        
        private void LoadAgentTemplate() 
        {
            EditorUtility.DisplayDialog("Info", "Agent template loading feature coming soon!", "OK");
        }
        
        private void SaveAgentPreset() 
        {
            EditorUtility.DisplayDialog("Info", "Agent preset saving feature coming soon!", "OK");
        }
        
        private void CreateNewExtension() 
        {
            ExtensionGeneratorWindow.ShowWindow();
        }
        
        private void RefreshExtensions() 
        {
            AssetDatabase.Refresh();
            RefreshData();
            EditorUtility.DisplayDialog("Refreshed", "Extensions list has been refreshed!", "OK");
        }
        
        private void ConfigureAPIKeys() 
        {
            APIConfigurationWindow.ShowWindow();
        }
        
        private void TestProviderConnections() 
        {
            EditorUtility.DisplayDialog("Testing", "Connection test feature coming soon!", "OK");
        }
        
        private void ShowProviderStatus() 
        {
            ProviderStatusWindow.ShowWindow();
        }
        
        private void GenerateUsageReport() 
        {
            EditorUtility.DisplayDialog("Report", "Usage report generation feature coming soon!", "OK");
        }
        
        private void ShowCostAnalysis() 
        {
            EditorUtility.DisplayDialog("Analysis", "Cost analysis feature coming soon!", "OK");
        }
        
        private void ClearAnalyticsData() 
        {
            if (EditorUtility.DisplayDialog("Clear Data", "Are you sure you want to clear all analytics data?", "Yes", "No"))
            {
                PlayerPrefs.DeleteKey("AISDK_Analytics");
                EditorUtility.DisplayDialog("Cleared", "Analytics data has been cleared!", "OK");
            }
        }
        
        private void DrawExtensionSelector()
        {
            var availableExtensions = new string[] { "PersonalityExtension", "CurrentTimeExtension", "WeatherExtension", "CalculatorExtension" };
            
            foreach (var ext in availableExtensions)
            {
                var isSelected = _selectedExtensions.Contains(ext);
                var newSelected = EditorGUILayout.Toggle(ext, isSelected);
                
                if (newSelected && !isSelected)
                {
                    _selectedExtensions.Add(ext);
                }
                else if (!newSelected && isSelected)
                {
                    _selectedExtensions.Remove(ext);
                }
            }
        }
        
        private void DrawAgentsList()
        {
            if (_agentList.Count == 0)
            {
                EditorGUILayout.HelpBox("No agents created yet. Create your first agent above!", MessageType.Info);
                return;
            }
            
            foreach (var agent in _agentList)
            {
                EditorGUILayout.BeginVertical(_cardStyle);
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Label($"🤖 {agent.Name} ({agent.Type})", _labelStyle);
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(40)))
                {
                    // Edit agent functionality
                }
                
                if (GUILayout.Button("❌", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    // Delete agent functionality
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawExtensionsList()
        {
            var extensions = new string[] { "PersonalityExtension", "CurrentTimeExtension", "WeatherExtension", "CalculatorExtension" };
            
            foreach (var ext in extensions)
            {
                EditorGUILayout.BeginVertical(_cardStyle);
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Label($"🔌 {ext}", _labelStyle);
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(40)))
                {
                    // Edit extension functionality
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawProviderSettings()
        {
            GUILayout.Label("API Configuration:", EditorStyles.boldLabel);
            
            var providers = Enum.GetValues(typeof(AIProviderType)).Cast<AIProviderType>();
            foreach (var provider in providers)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(provider.ToString(), GUILayout.Width(100));
                
                var hasKey = !string.IsNullOrEmpty(PlayerPrefs.GetString($"AISDK_{provider}_KEY"));
                var status = hasKey ? "✅" : "❌";
                GUILayout.Label(status, GUILayout.Width(20));
                
                if (GUILayout.Button("Configure", EditorStyles.miniButton, GUILayout.Width(70)))
                {
                    APIConfigurationWindow.ShowWindow();
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawAnalyticsCards()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Usage Card
            EditorGUILayout.BeginVertical(_cardStyle, GUILayout.Width(150));
            GUILayout.Label("📈 Usage Stats", EditorStyles.boldLabel);
            GUILayout.Label("Total Requests: 156", _labelStyle);
            GUILayout.Label("This Month: 42", _labelStyle);
            GUILayout.Label("Success Rate: 98.7%", _labelStyle);
            EditorGUILayout.EndVertical();
            
            // Cost Card
            EditorGUILayout.BeginVertical(_cardStyle, GUILayout.Width(150));
            GUILayout.Label("💰 Cost Analysis", EditorStyles.boldLabel);
            GUILayout.Label("This Month: $12.45", _labelStyle);
            GUILayout.Label("Last Month: $8.92", _labelStyle);
            GUILayout.Label("Avg per Request: $0.08", _labelStyle);
            EditorGUILayout.EndVertical();
            
            // Performance Card
            EditorGUILayout.BeginVertical(_cardStyle, GUILayout.Width(150));
            GUILayout.Label("⚡ Performance", EditorStyles.boldLabel);
            GUILayout.Label("Avg Response: 1.2s", _labelStyle);
            GUILayout.Label("Fastest: 0.8s", _labelStyle);
            GUILayout.Label("Slowest: 3.1s", _labelStyle);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Data Classes
        [Serializable]
        public class AgentInfo
        {
            public string Name;
            public AgentType Type;
            public bool IsActive;
        }
        
        [Serializable]
        public class ExtensionInfo
        {
            public string Name;
            public string Description;
            public ExtensionType Type;
        }
        
        public enum ExtensionType
        {
            Simple,
            Function,
            Complex
        }
        #endregion
    }
}
#endif