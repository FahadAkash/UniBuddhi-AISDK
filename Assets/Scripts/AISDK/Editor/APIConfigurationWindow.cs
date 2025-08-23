#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Editor
{
    public class APIConfigurationWindow : EditorWindow
    {
        private Dictionary<AIProviderType, string> _apiKeys = new Dictionary<AIProviderType, string>();
        private Dictionary<TTSProviderType, string> _ttsApiKeys = new Dictionary<TTSProviderType, string>();
        private Vector2 _scrollPos;
        
        public static void ShowWindow()
        {
            var window = GetWindow<APIConfigurationWindow>(true, "🔑 API Configuration", true);
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        private void OnEnable()
        {
            LoadAPIKeys();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            DrawAIProviders();
            GUILayout.Space(20);
            DrawTTSProviders();
            EditorGUILayout.EndScrollView();
            DrawButtons();
        }
        
        private void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.15f, 0.2f));
            var style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            GUI.Label(rect, "🔑 API Configuration", style);
        }
        
        private void DrawAIProviders()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("🤖 AI Provider API Keys", EditorStyles.boldLabel);
            
            foreach (AIProviderType provider in Enum.GetValues(typeof(AIProviderType)))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(provider.ToString(), GUILayout.Width(100));
                
                if (!_apiKeys.ContainsKey(provider))
                    _apiKeys[provider] = "";
                
                _apiKeys[provider] = EditorGUILayout.PasswordField(_apiKeys[provider]);
                
                var hasKey = !string.IsNullOrEmpty(_apiKeys[provider]);
                GUILayout.Label(hasKey ? "✅" : "❌", GUILayout.Width(20));
                
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTTSProviders()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("🗣️ TTS Provider API Keys", EditorStyles.boldLabel);
            
            foreach (TTSProviderType provider in Enum.GetValues(typeof(TTSProviderType)))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(provider.ToString(), GUILayout.Width(100));
                
                if (!_ttsApiKeys.ContainsKey(provider))
                    _ttsApiKeys[provider] = "";
                
                _ttsApiKeys[provider] = EditorGUILayout.PasswordField(_ttsApiKeys[provider]);
                
                var hasKey = !string.IsNullOrEmpty(_ttsApiKeys[provider]);
                GUILayout.Label(hasKey ? "✅" : "❌", GUILayout.Width(20));
                
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Reset All", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset", "Reset all API keys?", "Yes", "No"))
                    ResetAllKeys();
            }
            
            if (GUILayout.Button("💾 Save & Close", GUILayout.Height(30)))
            {
                SaveAPIKeys();
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void LoadAPIKeys()
        {
            foreach (AIProviderType provider in Enum.GetValues(typeof(AIProviderType)))
                _apiKeys[provider] = PlayerPrefs.GetString($"AISDK_{provider}_KEY", "");
            
            foreach (TTSProviderType provider in Enum.GetValues(typeof(TTSProviderType)))
                _ttsApiKeys[provider] = PlayerPrefs.GetString($"AISDK_TTS_{provider}_KEY", "");
        }
        
        private void SaveAPIKeys()
        {
            foreach (var kvp in _apiKeys)
                PlayerPrefs.SetString($"AISDK_{kvp.Key}_KEY", kvp.Value);
            
            foreach (var kvp in _ttsApiKeys)
                PlayerPrefs.SetString($"AISDK_TTS_{kvp.Key}_KEY", kvp.Value);
            
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("Saved", "API keys saved successfully!", "OK");
        }
        
        private void ResetAllKeys()
        {
            foreach (var provider in _apiKeys.Keys.ToArray())
            {
                _apiKeys[provider] = "";
                PlayerPrefs.DeleteKey($"AISDK_{provider}_KEY");
            }
            
            foreach (var provider in _ttsApiKeys.Keys.ToArray())
            {
                _ttsApiKeys[provider] = "";
                PlayerPrefs.DeleteKey($"AISDK_TTS_{provider}_KEY");
            }
            PlayerPrefs.Save();
        }
    }
}
#endif