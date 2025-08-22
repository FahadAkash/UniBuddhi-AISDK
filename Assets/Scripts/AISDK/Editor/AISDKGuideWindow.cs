#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniBuddhi.Core;

namespace UniBuddhi.Editor
{
	public class AISDKGuideWindow : EditorWindow
	{
		private const string WindowTitle = "UniBuddhi AI SDK Guide";
		private const float HeaderHeight = 120f;
		private const float SectionSpacing = 12f;
		private const float CardPadding = 10f;
		private const float PulseSpeed = 2.0f;

		private Vector2 _scroll;
		private float _animT;
		private double _lastTime;

		private GUIStyle _titleStyle;
		private GUIStyle _subtitleStyle;
		private GUIStyle _sectionTitleStyle;
		private GUIStyle _cardStyle;
		private GUIStyle _richLabel;
		private GUIStyle _buttonStyleLarge;

		[MenuItem("Tools/UniBuddhi AI SDK/✨ Open SDK Guide", priority = 0)]
		public static void Open()
		{
			var window = GetWindow<AISDKGuideWindow>(utility: false, title: WindowTitle, focus: true);
			window.minSize = new Vector2(720, 540);
			window.Show();
		}

		[MenuItem("Tools/UniBuddhi AI SDK/Add UniBuddhiCore to Scene", priority = 10)]
		public static void AddCoreToSceneMenu()
		{
			EnsureCoreInScene();
		}

		private void OnEnable()
		{
			_lastTime = EditorApplication.timeSinceStartup;
			EditorApplication.update += OnEditorUpdate;
		}

		private void OnDisable()
		{
			EditorApplication.update -= OnEditorUpdate;
		}

		private void OnEditorUpdate()
		{
			double now = EditorApplication.timeSinceStartup;
			double delta = now - _lastTime;
			_lastTime = now;
			_animT += (float)delta * PulseSpeed;
			Repaint();
		}

		private void BuildStyles()
		{
			_titleStyle = new GUIStyle(EditorStyles.label)
			{
				fontSize = 26,
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleLeft,
				richText = true
			};
			_subtitleStyle = new GUIStyle(EditorStyles.label)
			{
				fontSize = 12,
				alignment = TextAnchor.MiddleLeft,
				normal = { textColor = new Color(1f, 1f, 1f, 0.85f) }
			};
			_sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				fontSize = 14,
				fontStyle = FontStyle.Bold,
				richText = true
			};
			_cardStyle = new GUIStyle(EditorStyles.helpBox)
			{
				padding = new RectOffset(12, 12, 12, 12)
			};
			_richLabel = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
			_buttonStyleLarge = new GUIStyle(GUI.skin.button)
			{
				fontSize = 13,
				alignment = TextAnchor.MiddleCenter,
				fixedHeight = 32
			};
		}

		private void OnGUI()
		{
			// Ensure styles exist even if domain reload timing skips OnEnable
			if (_titleStyle == null || _buttonStyleLarge == null || _richLabel == null)
			{
				BuildStyles();
			}
			DrawHeader();
			_scroll = EditorGUILayout.BeginScrollView(_scroll);
			DrawQuickActions();
			GUILayout.Space(SectionSpacing);
			DrawSetupSteps();
			GUILayout.Space(SectionSpacing);
			DrawExamplesAndDocs();
			GUILayout.Space(SectionSpacing);
			DrawTroubleshooting();
			EditorGUILayout.EndScrollView();
		}

		private void DrawHeader()
		{
			// Reserve a full-width rectangle with fixed height using GUILayout
			Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(HeaderHeight), GUILayout.ExpandWidth(true));
			DrawGradient(r, new Color(0.13f, 0.16f, 0.22f), new Color(0.09f, 0.11f, 0.15f));

			GUILayout.BeginArea(r);
			GUILayout.Space(10);
			GUILayout.Label("<size=26>🤖 UniBuddhi AI SDK — <color=#7cd7ff>Welcome & Guide</color></size>", _titleStyle);
			GUILayout.Space(4);
			GUILayout.Label("Multi-provider AI, agents, TTS, and extensions for Unity", _subtitleStyle);
			GUILayout.EndArea();
		}

		private void DrawQuickActions()
		{
			EditorGUILayout.LabelField("🚀 Quick Actions", _sectionTitleStyle);
			EditorGUILayout.BeginVertical(_cardStyle);
			GUILayout.Space(CardPadding);

			EditorGUILayout.BeginHorizontal();
			if (AnimatedButton("➕ Add AISDKCore to Scene"))
			{
				EnsureCoreInScene();
			}
			if (GUILayout.Button("🧪 Run Example Scene (UI)", _buttonStyleLarge))
			{
				OpenExampleScene("Assets/Scripts/AISDK/Examples/ExampleScene.unity");
			}
			if (GUILayout.Button("🎭 Open Personality Demo", _buttonStyleLarge))
			{
				OpenOrPing("Assets/Scripts/AISDK/Examples/PersonalityExampleUsage.cs");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(6);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("📄 Open Main README", _buttonStyleLarge))
			{
				OpenOrPing("Assets/Scripts/AISDK/README.md");
			}
			if (GUILayout.Button("🎭 Personality Guide", _buttonStyleLarge))
			{
				OpenOrPing("Assets/Scripts/AISDK/Examples/README_PersonalityExtension.md");
			}
			if (GUILayout.Button("📁 Open Examples Folder", _buttonStyleLarge))
			{
				ShowInProject("Assets/Scripts/AISDK/Examples");
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(CardPadding);
			EditorGUILayout.EndVertical();
		}

		private void DrawSetupSteps()
		{
			EditorGUILayout.LabelField("🧭 Getting Started", _sectionTitleStyle);
			EditorGUILayout.BeginVertical(_cardStyle);

			EditorGUILayout.LabelField(
				"<b>1.</b> Add <b>AISDKCore</b> to your scene (or use the button above).\n" +
				"<b>2.</b> In the Inspector, choose your AI Provider and enter your API key.\n" +
				"<b>3.</b> (Optional) Enable TTS and set ElevenLabs API if desired.\n" +
				"<b>4.</b> (Optional) Add extensions like <i>CurrentTime</i> or <i>Personality</i>.\n" +
				"<b>5.</b> Use <b>AISDKExample</b> or <b>CompleteAIExample</b> to try it out.",
				_richLabel);

			GUILayout.Space(6);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("🧩 Add Personality Extension", _buttonStyleLarge))
			{
				AddPersonalityExtension();
			}
			if (GUILayout.Button("⏱️ Add Current Time Extension", _buttonStyleLarge))
			{
				AddCurrentTimeExtension();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(CardPadding);
			EditorGUILayout.EndVertical();
		}

		private void DrawExamplesAndDocs()
		{
			EditorGUILayout.LabelField("📚 Examples & Docs", _sectionTitleStyle);
			EditorGUILayout.BeginVertical(_cardStyle);

			EditorGUILayout.LabelField(
				"• <b>CompleteAIExample</b>: Full UI flow including streaming & TTS\n" +
				"• <b>PersonalityExampleUsage</b>: Personalities with custom prompts\n" +
				"• <b>AISDKExample</b>: Minimal quickstart example\n",
				_richLabel);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("🔎 Open Complete Example", _buttonStyleLarge))
			{
				OpenOrPing("Assets/Scripts/AISDK/Examples/CompleteAIExample.cs");
			}
			if (GUILayout.Button("🧪 Open Minimal Example", _buttonStyleLarge))
			{
				OpenOrPing("Assets/Scripts/AISDK/Examples/AISDKExample.cs");
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(CardPadding);
			EditorGUILayout.EndVertical();
		}

		private void DrawTroubleshooting()
		{
			EditorGUILayout.LabelField("🛠️ Troubleshooting", _sectionTitleStyle);
			EditorGUILayout.BeginVertical(_cardStyle);

			EditorGUILayout.LabelField(
				"• If compilation fails, check Console for missing API keys or namespaces.\n" +
				"• Ensure provider placeholders exist if you disable some providers.\n" +
				"• Use the README for coroutine patterns used by the SDK.\n" +
				"• Report issues with steps to reproduce.\n",
				_richLabel);

			if (GUILayout.Button("🧰 Re-run Compilation Check", _buttonStyleLarge))
			{
				// Ping README or guide users to your CI; in-Editor compilation happens automatically
				EditorUtility.DisplayDialog("Compilation", "Scripts will auto-compile on change. Check the Console for errors.", "OK");
			}

			GUILayout.Space(CardPadding);
			EditorGUILayout.EndVertical();
		}

		private bool AnimatedButton(string label)
		{
			float pulse = 0.5f + 0.5f * Mathf.Sin(_animT);
			Color baseColor = new Color(0.20f, 0.55f, 0.95f);
			Color hoverColor = Color.Lerp(baseColor, Color.white, 0.15f * pulse);
			Color original = GUI.color;
			GUI.color = hoverColor;
			bool clicked = GUILayout.Button(label, _buttonStyleLarge);
			GUI.color = original;
			return clicked;
		}

		private static void EnsureCoreInScene()
		{
			var core = GameObject.FindObjectOfType<AISDKCore>();
			if (core == null)
			{
				var go = new GameObject("AISDKCore");
				go.AddComponent<AISDKCore>();
				Undo.RegisterCreatedObjectUndo(go, "Create AISDKCore");
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				EditorGUIUtility.PingObject(go);
			}
			else
			{
				EditorGUIUtility.PingObject(core.gameObject);
			}
		}

		private static void AddPersonalityExtension()
		{
			var core = GameObject.FindObjectOfType<AISDKCore>();
			if (core == null)
			{
				EnsureCoreInScene();
				core = GameObject.FindObjectOfType<AISDKCore>();
			}
			var existing = GameObject.FindObjectOfType<UniBuddhi.Core.Extensions.PersonalityAgentExtension>();
			if (existing == null && core != null)
			{
				var go = new GameObject("PersonalityAgentExtension");
				go.transform.SetParent(core.transform);
				go.AddComponent<UniBuddhi.Core.Extensions.PersonalityAgentExtension>();
				Undo.RegisterCreatedObjectUndo(go, "Add PersonalityAgentExtension");
				EditorGUIUtility.PingObject(go);
			}
			else if (existing != null)
			{
				EditorGUIUtility.PingObject(existing.gameObject);
			}
		}

		private static void AddCurrentTimeExtension()
		{
			var core = GameObject.FindObjectOfType<AISDKCore>();
			if (core == null)
			{
				EnsureCoreInScene();
				core = GameObject.FindObjectOfType<AISDKCore>();
			}
			var existing = GameObject.FindObjectOfType<UniBuddhi.Core.Extensions.CurrentTimeExtension>();
			if (existing == null && core != null)
			{
				var go = new GameObject("CurrentTimeExtension");
				go.transform.SetParent(core.transform);
				go.AddComponent<UniBuddhi.Core.Extensions.CurrentTimeExtension>();
				Undo.RegisterCreatedObjectUndo(go, "Add CurrentTimeExtension");
				EditorGUIUtility.PingObject(go);
			}
			else if (existing != null)
			{
				EditorGUIUtility.PingObject(existing.gameObject);
			}
		}

		private static void OpenExampleScene(string path)
		{
			if (File.Exists(path))
			{
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				{
					EditorSceneManager.OpenScene(path);
				}
			}
			else
			{
				EditorUtility.DisplayDialog("Scene Not Found", $"Could not find scene at:\n{path}", "OK");
			}
		}

		private static void OpenOrPing(string path)
		{
			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			if (obj != null)
			{
				Selection.activeObject = obj;
				EditorGUIUtility.PingObject(obj);
			}
			else
			{
				EditorUtility.DisplayDialog("File Not Found", $"Could not find:\n{path}", "OK");
			}
		}

		private static void ShowInProject(string path)
		{
			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			if (obj != null)
			{
				EditorUtility.RevealInFinder(path);
			}
			else
			{
				EditorUtility.DisplayDialog("Folder Not Found", $"Could not open:\n{path}", "OK");
			}
		}

		private static void DrawGradient(Rect rect, Color top, Color bottom)
		{
			if (Event.current.type != EventType.Repaint) return;
			var tex = GetGradientTexture(top, bottom);
			GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill);
		}

		private static Texture2D _gradientCache;
		private static Color _gTop, _gBottom;
		private static Texture2D GetGradientTexture(Color top, Color bottom)
		{
			if (_gradientCache == null || _gTop != top || _gBottom != bottom)
			{
				_gTop = top; _gBottom = bottom;
				_gradientCache = new Texture2D(1, 64);
				_gradientCache.wrapMode = TextureWrapMode.Clamp;
				for (int y = 0; y < 64; y++)
				{
					float t = y / 63f;
					_gradientCache.SetPixel(0, y, Color.Lerp(top, bottom, t));
				}
				_gradientCache.Apply();
			}
			return _gradientCache;
		}
	}
}
#endif
