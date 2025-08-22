using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Extensions
{
    /// <summary>
    /// Provides current time and date information
    /// </summary>
    public class CurrentTimeExtension : BaseExtension
    {
        #region Properties
        public override string Name => "CurrentTime";
        public override string Version => "1.0.0";
        public override string Description => "Provides current time and date information";
        #endregion

        #region Inspector Settings
        [Header("Current Time Extension Settings")]
        [SerializeField] private bool enabledForAll = false;
        [SerializeField] private string[] triggerKeywords = { 
            "time", "current time", "what time", "clock", "hour", "minute", 
            "date", "today", "current date", "day", "month", "year",
            "now", "current", "moment"
        };
        [SerializeField] private bool includeTimezone = true;
        [SerializeField] private bool includeUnixTimestamp = false;
        [SerializeField] private string dateFormat = "dddd, MMMM dd, yyyy";
        [SerializeField] private string timeFormat = "HH:mm:ss";
        #endregion

        #region BaseExtension Implementation
        protected override IEnumerator ProcessPreprocess(string userMessage, Action<string> onComplete)
        {
            // Check if this extension should respond
            bool shouldRespond = enabledForAll;
            
            if (!shouldRespond)
            {
                string lowerMessage = userMessage.ToLowerInvariant();
                foreach (string keyword in triggerKeywords)
                {
                    if (lowerMessage.Contains(keyword.ToLowerInvariant()))
                    {
                        shouldRespond = true;
                        break;
                    }
                }
            }

            if (!shouldRespond)
            {
                onComplete?.Invoke("");
                yield break;
            }

            // Get current time and date
            DateTime now = DateTime.Now;
            
            // Build time context
            var contextBuilder = new System.Text.StringBuilder();
            contextBuilder.AppendLine("Current Time Information:");
            contextBuilder.AppendLine($"Time: {now.ToString(timeFormat)}");
            contextBuilder.AppendLine($"Date: {now.ToString(dateFormat)}");
            contextBuilder.AppendLine($"Day of Week: {now.ToString("dddd")}");
            contextBuilder.AppendLine($"Month: {now.ToString("MMMM")}");
            contextBuilder.AppendLine($"Year: {now.Year}");

            if (includeTimezone)
            {
                contextBuilder.AppendLine($"Timezone: {TimeZoneInfo.Local.DisplayName}");
            }

            if (includeUnixTimestamp)
            {
                contextBuilder.AppendLine($"Unix Timestamp: {((DateTimeOffset)now).ToUnixTimeSeconds()}");
            }

            onComplete?.Invoke(contextBuilder.ToString());
        }

        protected override IEnumerator ProcessPostprocess(string modelText, Action<ExtensionResult> onComplete)
        {
            // No post-processing needed for time extension
            onComplete?.Invoke(new ExtensionResult(Name, modelText));
            yield break;
        }

        protected override IEnumerator PerformTest(Action<bool> onComplete)
        {
            try
            {
                // Test time retrieval
                DateTime testTime = DateTime.Now;
                string testContext = $"Test Time: {testTime.ToString(timeFormat)}";
                
                if (!string.IsNullOrEmpty(testContext))
                {
                    LogInfo("Time extension test passed");
                    onComplete?.Invoke(true);
                }
                else
                {
                    LogError("Time extension test failed - no context generated");
                    onComplete?.Invoke(false);
                }
            }
            catch (Exception ex)
            {
                LogError($"Time extension test error: {ex.Message}");
                onComplete?.Invoke(false);
            }
            yield break;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get current time as formatted string
        /// </summary>
        public string GetCurrentTime()
        {
            return DateTime.Now.ToString(timeFormat);
        }

        /// <summary>
        /// Get current date as formatted string
        /// </summary>
        public string GetCurrentDate()
        {
            return DateTime.Now.ToString(dateFormat);
        }

        /// <summary>
        /// Get current timezone
        /// </summary>
        public string GetCurrentTimezone()
        {
            return TimeZoneInfo.Local.DisplayName;
        }

        /// <summary>
        /// Get Unix timestamp
        /// </summary>
        public long GetUnixTimestamp()
        {
            return ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
        }
        #endregion
    }
}
