#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UniBuddhi.Editor
{
    public class ExtensionGeneratorWindow : EditorWindow
    {
        #region Constants
        private const string WindowTitle = "🔧 Extension Generator";
        private const float WindowWidth = 700f;
        private const float WindowHeight = 600f;
        #endregion
        
        #region Private Fields
        private string _extensionName = "MyExtension";
        private string _extensionDescription = "Custom extension for AI agents";
        private ExtensionTemplate _selectedTemplate = ExtensionTemplate.Basic;
        private string _namespace = "UniBuddhi.Core.Extensions";
        private bool _includeFunction = false;
        private bool _includePreprocess = true;
        private bool _includePostprocess = true;
        private bool _includeUI = false;
        
        private Vector2 _scrollPos;
        private string _generatedCode = "";
        
        // Styles
        private GUIStyle _headerStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _codeStyle;
        #endregion
        
        #region Enums
        public enum ExtensionTemplate
        {
            Basic,
            Function,
            DataProcessor,
            APIIntegration,
            Custom
        }
        #endregion
        
        #region Public Methods
        [MenuItem("Tools/UniBuddhi AI SDK/🔧 Extension Generator", priority = 3)]
        public static void ShowWindow()
        {
            var window = GetWindow<ExtensionGeneratorWindow>(WindowTitle);
            window.minSize = new Vector2(WindowWidth, WindowHeight);
            window.Show();
        }
        #endregion
        
        #region Unity Events
        private void OnEnable()
        {
            InitializeStyles();
            GeneratePreview();
        }
        
        private void OnGUI()
        {
            if (_headerStyle == null) InitializeStyles();
            
            DrawHeader();
            
            EditorGUILayout.BeginHorizontal();
            DrawConfigurationPanel();
            DrawPreviewPanel();
            EditorGUILayout.EndHorizontal();
            
            DrawGenerateButton();
        }
        #endregion
        
        #region Style Initialization
        private void InitializeStyles()
        {
            _headerStyle = new GUIStyle(EditorStyles.toolbar)
            {
                fixedHeight = 40f,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            
            _cardStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 12, 12),
                margin = new RectOffset(5, 5, 5, 5)
            };
            
            _titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                richText = true
            };
            
            _codeStyle = new GUIStyle(EditorStyles.textArea)
            {
                font = Font.CreateDynamicFontFromOSFont("Consolas", 12),
                richText = false
            };
        }
        #endregion
        
        #region Drawing Methods
        private void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.15f, 0.2f));
            GUI.Label(rect, "🔧 Extension Generator", _headerStyle);
        }
        
        private void DrawConfigurationPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            
            // Basic Settings
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("⚙️ <b>Basic Settings</b>", _titleStyle);
            
            _extensionName = EditorGUILayout.TextField("Extension Name", _extensionName);
            _extensionDescription = EditorGUILayout.TextField("Description", _extensionDescription);
            _namespace = EditorGUILayout.TextField("Namespace", _namespace);
            
            EditorGUILayout.EndVertical();
            
            // Template Selection
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("📋 <b>Template</b>", _titleStyle);
            
            var newTemplate = (ExtensionTemplate)EditorGUILayout.EnumPopup("Template Type", _selectedTemplate);
            if (newTemplate != _selectedTemplate)
            {
                _selectedTemplate = newTemplate;
                GeneratePreview();
            }
            
            DrawTemplateDescription();
            
            EditorGUILayout.EndVertical();
            
            // Features
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("🎛️ <b>Features</b>", _titleStyle);
            
            var newIncludeFunction = EditorGUILayout.Toggle("Function Calling", _includeFunction);
            var newIncludePreprocess = EditorGUILayout.Toggle("Preprocessing", _includePreprocess);
            var newIncludePostprocess = EditorGUILayout.Toggle("Postprocessing", _includePostprocess);
            var newIncludeUI = EditorGUILayout.Toggle("UI Components", _includeUI);
            
            if (newIncludeFunction != _includeFunction || 
                newIncludePreprocess != _includePreprocess ||
                newIncludePostprocess != _includePostprocess ||
                newIncludeUI != _includeUI)
            {
                _includeFunction = newIncludeFunction;
                _includePreprocess = newIncludePreprocess;
                _includePostprocess = newIncludePostprocess;
                _includeUI = newIncludeUI;
                GeneratePreview();
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPreviewPanel()
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginVertical(_cardStyle);
            GUILayout.Label("👀 <b>Code Preview</b>", _titleStyle);
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));
            _generatedCode = EditorGUILayout.TextArea(_generatedCode, _codeStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTemplateDescription()
        {
            var descriptions = new Dictionary<ExtensionTemplate, string>
            {
                { ExtensionTemplate.Basic, "Simple extension with preprocessing and postprocessing capabilities" },
                { ExtensionTemplate.Function, "Extension with function calling capabilities for AI agents" },
                { ExtensionTemplate.DataProcessor, "Extension for processing and transforming data" },
                { ExtensionTemplate.APIIntegration, "Extension for integrating with external APIs" },
                { ExtensionTemplate.Custom, "Fully customizable extension template" }
            };
            
            if (descriptions.ContainsKey(_selectedTemplate))
            {
                EditorGUILayout.HelpBox(descriptions[_selectedTemplate], MessageType.Info);
            }
        }
        
        private void DrawGenerateButton()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("🔄 Refresh Preview", GUILayout.Height(30)))
            {
                GeneratePreview();
            }
            
            if (GUILayout.Button("💾 Generate & Save", GUILayout.Height(30)))
            {
                GenerateAndSaveExtension();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        #endregion
        
        #region Code Generation
        private void GeneratePreview()
        {
            _generatedCode = GenerateExtensionCode();
        }
        
        private string GenerateExtensionCode()
        {
            var sb = new StringBuilder();
            
            // Using statements
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UniBuddhi.Core.Interfaces;");
            sb.AppendLine("using UniBuddhi.Core.Models;");
            sb.AppendLine("using UniBuddhi.Core.Extensions;");
            
            if (_includeFunction)
            {
                sb.AppendLine("using System.Collections.Generic;");
            }
            
            sb.AppendLine();
            
            // Namespace
            sb.AppendLine($"namespace {_namespace}");
            sb.AppendLine("{");
            
            // Class declaration
            var baseClass = _includeFunction ? "BaseFunctionExtension" : "BaseExtension";
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {_extensionDescription}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public class {_extensionName} : {baseClass}");
            sb.AppendLine("    {");
            
            // Properties
            sb.AppendLine($"        public override string ExtensionName => \"{_extensionName}\";");
            sb.AppendLine($"        public override string Description => \"{_extensionDescription}\";");
            sb.AppendLine();
            
            // Feature flags
            if (_includePreprocess)
            {
                sb.AppendLine("        public override bool CanPreprocess => true;");
            }
            if (_includePostprocess)
            {
                sb.AppendLine("        public override bool CanPostprocess => true;");
            }
            sb.AppendLine();
            
            // Generate methods based on template
            switch (_selectedTemplate)
            {
                case ExtensionTemplate.Basic:
                    GenerateBasicMethods(sb);
                    break;
                case ExtensionTemplate.Function:
                    GenerateFunctionMethods(sb);
                    break;
                case ExtensionTemplate.DataProcessor:
                    GenerateDataProcessorMethods(sb);
                    break;
                case ExtensionTemplate.APIIntegration:
                    GenerateAPIMethods(sb);
                    break;
                case ExtensionTemplate.Custom:
                    GenerateCustomMethods(sb);
                    break;
            }
            
            // Close class and namespace
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        private void GenerateBasicMethods(StringBuilder sb)
        {
            if (_includePreprocess)
            {
                sb.AppendLine("        public override string Preprocess(string input, IAgent agent)");
                sb.AppendLine("        {");
                sb.AppendLine("            // Add your preprocessing logic here");
                sb.AppendLine("            Debug.Log($\"[{ExtensionName}] Preprocessing: {input}\");");
                sb.AppendLine("            return input;");
                sb.AppendLine("        }");
                sb.AppendLine();
            }
            
            if (_includePostprocess)
            {
                sb.AppendLine("        public override string Postprocess(string output, IAgent agent)");
                sb.AppendLine("        {");
                sb.AppendLine("            // Add your postprocessing logic here");
                sb.AppendLine("            Debug.Log($\"[{ExtensionName}] Postprocessing: {output}\");");
                sb.AppendLine("            return output;");
                sb.AppendLine("        }");
            }
        }
        
        private void GenerateFunctionMethods(StringBuilder sb)
        {
            sb.AppendLine("        [FunctionCall(\"example_function\", \"Example function description\")]");
            sb.AppendLine("        public FunctionResult ExampleFunction(string parameter)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                // Implement your function logic here");
            sb.AppendLine("                var result = $\"Processed: {parameter}\";");
            sb.AppendLine("                return new FunctionResult(true, result);");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                return new FunctionResult(false, ex.Message);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }
        
        private void GenerateDataProcessorMethods(StringBuilder sb)
        {
            sb.AppendLine("        public T ProcessData<T>(T data) where T : class");
            sb.AppendLine("        {");
            sb.AppendLine("            // Implement data processing logic");
            sb.AppendLine("            return data;");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            if (_includePreprocess)
            {
                GenerateBasicMethods(sb);
            }
        }
        
        private void GenerateAPIMethods(StringBuilder sb)
        {
            sb.AppendLine("        private const string API_BASE_URL = \"https://api.example.com\";");
            sb.AppendLine();
            sb.AppendLine("        public IEnumerator CallAPIAsync(string endpoint, Action<string> callback)");
            sb.AppendLine("        {");
            sb.AppendLine("            var url = $\"{API_BASE_URL}/{endpoint}\";");
            sb.AppendLine("            using (var request = UnityEngine.Networking.UnityWebRequest.Get(url))");
            sb.AppendLine("            {");
            sb.AppendLine("                yield return request.SendWebRequest();");
            sb.AppendLine("                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)");
            sb.AppendLine("                {");
            sb.AppendLine("                    callback?.Invoke(request.downloadHandler.text);");
            sb.AppendLine("                }");
            sb.AppendLine("                else");
            sb.AppendLine("                {");
            sb.AppendLine("                    Debug.LogError($\"API Error: {request.error}\");");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }
        
        private void GenerateCustomMethods(StringBuilder sb)
        {
            sb.AppendLine("        // Add your custom methods here");
            sb.AppendLine();
            
            if (_includePreprocess)
            {
                GenerateBasicMethods(sb);
            }
        }
        
        private void GenerateAndSaveExtension()
        {
            if (string.IsNullOrEmpty(_extensionName))
            {
                EditorUtility.DisplayDialog("Error", "Extension name cannot be empty!", "OK");
                return;
            }
            
            var fileName = _extensionName + ".cs";
            var path = EditorUtility.SaveFilePanel("Save Extension", "Assets/Scripts/AISDK/Core/Extensions", fileName, "cs");
            
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, _generatedCode);
                
                // Refresh asset database if saved in project
                if (path.StartsWith(Application.dataPath))
                {
                    AssetDatabase.Refresh();
                }
                
                EditorUtility.DisplayDialog("Success!", 
                    $"Extension '{_extensionName}' has been generated successfully!\\n\\nLocation: {path}",
                    "Great!");
            }
        }
        #endregion
    }
}
#endif