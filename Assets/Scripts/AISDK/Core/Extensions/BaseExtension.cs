using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AISDK.Core.Interfaces;
using AISDK.Core.Models;

namespace AISDK.Core.Extensions
{
    /// <summary>
    /// Base extension class implementing IAgentExtension interface
    /// </summary>
    public abstract class BaseExtension : MonoBehaviour, IAgentExtension
    {
        #region Properties
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }
        
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 0;
        public ExtensionConfig Config { get; protected set; }
        #endregion

        #region Protected Fields
        protected Dictionary<string, object> _metadata = new Dictionary<string, object>();
        protected Dictionary<string, object> _statistics = new Dictionary<string, object>();
        protected bool _isInitialized = false;
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            InitializeStatistics();
        }

        protected virtual void Start()
        {
            if (!_isInitialized)
            {
                Initialize(new ExtensionConfig(Name, Version, IsEnabled, Priority));
            }
        }
        #endregion

        #region IAgentExtension Implementation
        public virtual void Initialize(ExtensionConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            
            // Apply configuration
            IsEnabled = config.Enabled;
            Priority = config.Priority;
            
            // Initialize metadata
            _metadata["name"] = Name;
            _metadata["version"] = Version;
            _metadata["description"] = Description;
            _metadata["initialized"] = DateTime.Now;
            
            _isInitialized = true;
            LogDebug($"Initialized extension: {Name} v{Version}");
        }

        public virtual IEnumerator Preprocess(string userMessage, Action<ExtensionContext> onContextReady)
        {
            onContextReady = onContextReady ?? (_ => { });
            
            if (!IsEnabled || !_isInitialized)
            {
                onContextReady(new ExtensionContext(Name, "", Priority));
                yield break;
            }

            if (!ShouldRespond(userMessage))
            {
                onContextReady(new ExtensionContext(Name, "", Priority));
                yield break;
            }

            string context = "";
            bool processComplete = false;
            bool processError = false;

            StartCoroutine(ProcessPreprocess(userMessage, (c) => 
            {
                context = c;
                processComplete = true;
            }));

            while (!processComplete)
            {
                yield return null;
            }

            if (string.IsNullOrEmpty(context))
            {
                processError = true;
            }

            var extensionContext = new ExtensionContext(Name, context, Priority);
            
            if (!string.IsNullOrEmpty(context))
            {
                _statistics["preprocess_calls"] = (int)_statistics["preprocess_calls"] + 1;
                LogDebug($"Preprocess generated context: {context.Substring(0, Mathf.Min(100, context.Length))}...");
            }
            
            onContextReady(extensionContext);
        }

        public virtual IEnumerator Postprocess(string modelText, Action<ExtensionResult> onResultReady)
        {
            onResultReady = onResultReady ?? (_ => { });
            
            if (!IsEnabled || !_isInitialized)
            {
                onResultReady(new ExtensionResult(Name, modelText));
                yield break;
            }

            ExtensionResult result = null;
            bool processComplete = false;

            StartCoroutine(ProcessPostprocess(modelText, (r) => 
            {
                result = r;
                processComplete = true;
            }));

            while (!processComplete)
            {
                yield return null;
            }

            if (result == null)
            {
                result = new ExtensionResult(Name, modelText);
            }

            if (result != null && result.WasModified)
            {
                _statistics["postprocess_modifications"] = (int)_statistics["postprocess_modifications"] + 1;
                LogDebug($"Postprocess modified text: {result.OriginalText.Substring(0, Mathf.Min(50, result.OriginalText.Length))}...");
            }
            
            onResultReady(result ?? new ExtensionResult(Name, modelText));
        }

        public virtual bool ShouldRespond(string userMessage)
        {
            // Default implementation - always respond if enabled
            return IsEnabled && !string.IsNullOrEmpty(userMessage);
        }

        public virtual Dictionary<string, object> GetMetadata()
        {
            return new Dictionary<string, object>(_metadata);
        }

        public virtual Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>(_statistics)
            {
                ["extension_name"] = Name,
                ["version"] = Version,
                ["is_enabled"] = IsEnabled,
                ["priority"] = Priority,
                ["is_initialized"] = _isInitialized
            };
            return stats;
        }

        public virtual bool ValidateConfig()
        {
            return _isInitialized && Config != null;
        }

        public virtual IEnumerator TestExtension(Action<bool, string> onComplete)
        {
            onComplete = onComplete ?? ((_, __) => { });
            
            bool result = false;
            bool testComplete = false;

            StartCoroutine(PerformTest((r) => 
            {
                result = r;
                testComplete = true;
            }));

            while (!testComplete)
            {
                yield return null;
            }

            onComplete(result, result ? "Test passed" : "Test failed");
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Process preprocessing - override in derived classes
        /// </summary>
        protected abstract IEnumerator ProcessPreprocess(string userMessage, Action<string> onComplete);

        /// <summary>
        /// Process postprocessing - override in derived classes
        /// </summary>
        protected abstract IEnumerator ProcessPostprocess(string modelText, Action<ExtensionResult> onComplete);

        /// <summary>
        /// Perform extension test - override in derived classes
        /// </summary>
        protected abstract IEnumerator PerformTest(Action<bool> onComplete);
        #endregion

        #region Protected Methods
        protected virtual void InitializeStatistics()
        {
            _statistics["preprocess_calls"] = 0;
            _statistics["postprocess_calls"] = 0;
            _statistics["postprocess_modifications"] = 0;
            _statistics["errors"] = 0;
            _statistics["created"] = DateTime.Now;
        }

        protected virtual void LogDebug(string message)
        {
            Debug.Log($"[AISDK] [{Name}] {message}");
        }

        protected virtual void LogWarning(string message)
        {
            Debug.LogWarning($"[AISDK] [{Name}] {message}");
        }

        protected virtual void LogError(string message)
        {
            Debug.LogError($"[AISDK] [{Name}] {message}");
            _statistics["errors"] = (int)_statistics["errors"] + 1;
        }

        protected virtual void LogInfo(string message)
        {
            Debug.Log($"[AISDK] [{Name}] {message}");
        }
        #endregion
    }
}
