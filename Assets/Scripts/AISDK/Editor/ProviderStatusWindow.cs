#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniBuddhi.Core;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Editor
{
    public class ProviderStatusWindow : EditorWindow
    {
        private Dictionary<AIProviderType, ConnectionStatus> _providerStatus = new Dictionary<AIProviderType, ConnectionStatus>();
        private Vector2 _scrollPos;
        private bool _isChecking = false;
        
        public enum ConnectionStatus
        {
            Unknown,
            Connected,
            Failed,
            Checking
        }
        
        public static void ShowWindow()
        {
            var window = GetWindow<ProviderStatusWindow>("📡 Provider Status");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }
        
        private void OnEnable()
        {
            InitializeStatus();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            DrawToolbar();
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            DrawProviderStatus();
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.15f, 0.2f));
            var style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            GUI.Label(rect, "📡 Provider Status Monitor", style);
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUI.enabled = !_isChecking;
            if (GUILayout.Button("🔄 Check All", EditorStyles.toolbarButton))
            {
                CheckAllProviders();
            }
            
            if (GUILayout.Button("🧪 Test Current", EditorStyles.toolbarButton))
            {
                TestCurrentProvider();
            }
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            
            if (_isChecking)
            {
                GUILayout.Label("⏳ Checking...", EditorStyles.toolbarButton);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawProviderStatus()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("🤖 AI Providers", EditorStyles.boldLabel);
            
            foreach (var provider in _providerStatus)
            {
                DrawProviderRow(provider.Key, provider.Value);
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Current provider highlight
            if (AISDKCore.Instance != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var currentProvider = AISDKCore.Instance.CurrentProvider;
                GUILayout.Label($"🎯 Current Provider: {currentProvider}", EditorStyles.boldLabel);
                
                if (_providerStatus.ContainsKey(currentProvider))
                {
                    var status = _providerStatus[currentProvider];
                    var statusText = GetStatusText(status);
                    var statusColor = GetStatusColor(status);
                    
                    var oldColor = GUI.color;
                    GUI.color = statusColor;
                    GUILayout.Label(statusText, EditorStyles.label);
                    GUI.color = oldColor;
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawProviderRow(AIProviderType provider, ConnectionStatus status)
        {
            EditorGUILayout.BeginHorizontal();
            
            var icon = GetProviderIcon(provider);
            GUILayout.Label($"{icon} {provider}", GUILayout.Width(150));
            
            var statusText = GetStatusText(status);
            var statusColor = GetStatusColor(status);
            
            var oldColor = GUI.color;
            GUI.color = statusColor;
            GUILayout.Label(statusText, GUILayout.Width(100));
            GUI.color = oldColor;
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Test", GUILayout.Width(50)))
            {
                TestProvider(provider);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private string GetProviderIcon(AIProviderType provider)
        {
            return provider switch
            {
                AIProviderType.OpenAI => "🤖",
                AIProviderType.Gemini => "💎",
                AIProviderType.Anthropic => "🧠",
                AIProviderType.Cohere => "🌊",
                AIProviderType.DeepSeek => "🔍",
                _ => "🔧"
            };
        }
        
        private string GetStatusText(ConnectionStatus status)
        {
            return status switch
            {
                ConnectionStatus.Connected => "✅ Connected",
                ConnectionStatus.Failed => "❌ Failed",
                ConnectionStatus.Checking => "⏳ Checking...",
                _ => "❓ Unknown"
            };
        }
        
        private Color GetStatusColor(ConnectionStatus status)
        {
            return status switch
            {
                ConnectionStatus.Connected => Color.green,
                ConnectionStatus.Failed => Color.red,
                ConnectionStatus.Checking => Color.yellow,
                _ => Color.gray
            };
        }
        
        private void InitializeStatus()
        {
            foreach (AIProviderType provider in Enum.GetValues(typeof(AIProviderType)))
            {
                _providerStatus[provider] = ConnectionStatus.Unknown;
            }
        }
        
        private void CheckAllProviders()
        {
            _isChecking = true;
            
            foreach (var provider in _providerStatus.Keys)
            {
                _providerStatus[provider] = ConnectionStatus.Checking;
            }
            
            Repaint();
            
            // Simulate checking (in real implementation, this would be async)
            EditorApplication.delayCall += () =>
            {
                foreach (var provider in _providerStatus.Keys.ToArray())
                {
                    var hasKey = !string.IsNullOrEmpty(PlayerPrefs.GetString($"AISDK_{provider}_KEY"));
                    _providerStatus[provider] = hasKey ? ConnectionStatus.Connected : ConnectionStatus.Failed;
                }
                
                _isChecking = false;
                Repaint();
            };
        }
        
        private void TestCurrentProvider()
        {
            if (AISDKCore.Instance != null)
            {
                TestProvider(AISDKCore.Instance.CurrentProvider);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "AISDKCore not found in scene!", "OK");
            }
        }
        
        private void TestProvider(AIProviderType provider)
        {
            _providerStatus[provider] = ConnectionStatus.Checking;
            Repaint();
            
            // Simulate testing
            EditorApplication.delayCall += () =>
            {
                var hasKey = !string.IsNullOrEmpty(PlayerPrefs.GetString($"AISDK_{provider}_KEY"));
                _providerStatus[provider] = hasKey ? ConnectionStatus.Connected : ConnectionStatus.Failed;
                Repaint();
                
                var result = hasKey ? "Connection successful!" : "No API key found!";
                EditorUtility.DisplayDialog($"{provider} Test", result, "OK");
            };
        }
    }
}
#endif