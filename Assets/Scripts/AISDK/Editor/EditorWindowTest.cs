#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UniBuddhi.Editor
{
    public class EditorWindowTest
    {
        [MenuItem("Tools/UniBuddhi AI SDK/🧪 Test AI SDK Window")]
        public static void TestAISDKWindow()
        {
            try
            {
                Debug.Log("Opening AISDKManagerWindow...");
                AISDKManagerWindow.ShowWindow();
                Debug.Log("✅ AISDKManagerWindow opened successfully! The window should now display with content.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to open AISDKManagerWindow: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
            }
        }
        
        [MenuItem("Tools/UniBuddhi AI SDK/📋 Show Window Info")]
        public static void ShowWindowInfo()
        {
            var windows = Resources.FindObjectsOfTypeAll<AISDKManagerWindow>();
            if (windows.Length > 0)
            {
                Debug.Log($"✅ Found {windows.Length} AISDKManagerWindow(s) open.");
                foreach (var window in windows)
                {
                    Debug.Log($"Window: {window.titleContent.text}, Position: {window.position}");
                }
            }
            else
            {
                Debug.Log("❌ No AISDKManagerWindow found. Try opening it first.");
            }
        }
    }
}
#endif