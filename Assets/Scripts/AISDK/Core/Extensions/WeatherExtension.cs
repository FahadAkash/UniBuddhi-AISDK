using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Extensions
{
    /// <summary>
    /// Weather extension - provides weather information and forecasts
    /// </summary>
    public class WeatherExtension : BaseExtension
    {
        [Header("Weather Settings")]
        [SerializeField] private string apiKey = "";
        [SerializeField] private string city = "New York";
        [SerializeField] private bool useMockData = true;
        
        [Header("Mock Weather Data")]
        [SerializeField] private string mockWeather = "Sunny";
        [SerializeField] private float mockTemperature = 25f;
        [SerializeField] private float mockHumidity = 60f;
        
        private Dictionary<string, object> _weatherCache = new Dictionary<string, object>();
        private float _lastUpdateTime = 0f;
        private const float CACHE_DURATION = 300f; // 5 minutes
        
        public override string Name => "WeatherExtension";
        public override string Version => "1.0.0";
        public override string Description => "Provides real-time weather information and forecasts";
        
        protected override void Start()
        {
            base.Start();
            
            // Initialize weather cache
            UpdateWeatherData();
        }
        
        protected override IEnumerator ProcessPreprocess(string userMessage, Action<string> onComplete)
        {
            if (ShouldRespond(userMessage))
            {
                var weatherInfo = GetWeatherInformation();
                var context = $"Current weather in {city}: {weatherInfo}";
                onComplete(context);
            }
            else
            {
                onComplete("");
            }
            yield break;
        }
        
        protected override IEnumerator ProcessPostprocess(string modelText, Action<ExtensionResult> onComplete)
        {
            // Add weather context to responses if relevant
            if (modelText.ToLower().Contains("weather") || modelText.ToLower().Contains("temperature"))
            {
                var weatherInfo = GetWeatherInformation();
                var enhancedText = $"{modelText}\n\n🌤️ Weather Update: {weatherInfo}";
                var result = new ExtensionResult(Name, modelText, enhancedText);
                onComplete(result);
            }
            else
            {
                var result = new ExtensionResult(Name, modelText, modelText);
                onComplete(result);
            }
            yield break;
        }
        
        public override bool ShouldRespond(string userMessage)
        {
            var lowerMessage = userMessage.ToLower();
            return lowerMessage.Contains("weather") || 
                   lowerMessage.Contains("temperature") || 
                   lowerMessage.Contains("forecast") ||
                   lowerMessage.Contains("rain") ||
                   lowerMessage.Contains("sunny") ||
                   lowerMessage.Contains("cold") ||
                   lowerMessage.Contains("hot");
        }
        
        protected override IEnumerator PerformTest(Action<bool> onComplete)
        {
            try
            {
                UpdateWeatherData();
                var weatherInfo = GetWeatherInformation();
                
                if (!string.IsNullOrEmpty(weatherInfo))
                {
                    onComplete(true);
                }
                else
                {
                    onComplete(false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WeatherExtension] Test failed: {ex.Message}");
                onComplete(false);
            }
            yield break;
        }
        
        /// <summary>
        /// Get current weather information
        /// </summary>
        public string GetWeatherInformation()
        {
            // Check if cache is valid
            if (Time.time - _lastUpdateTime > CACHE_DURATION)
            {
                UpdateWeatherData();
            }
            
            if (_weatherCache.ContainsKey("description") && _weatherCache.ContainsKey("temperature"))
            {
                var description = _weatherCache["description"] as string;
                var temperature = (float)_weatherCache["temperature"];
                var humidity = (float)_weatherCache["humidity"];
                
                return $"{description}, {temperature:F1}°C, Humidity: {humidity:F0}%";
            }
            
            return "Weather information unavailable";
        }
        
        /// <summary>
        /// Get weather forecast for specific days
        /// </summary>
        public string GetWeatherForecast(int days = 3)
        {
            if (useMockData)
            {
                var forecasts = new string[]
                {
                    "Today: Sunny, 25°C",
                    "Tomorrow: Partly Cloudy, 23°C", 
                    "Day after: Light Rain, 20°C"
                };
                
                return string.Join("\n", forecasts.Take(days));
            }
            
            // In real implementation, call weather API
            return "Forecast unavailable";
        }
        
        /// <summary>
        /// Update weather data
        /// </summary>
        private void UpdateWeatherData()
        {
            if (useMockData)
            {
                // Use mock data for testing
                _weatherCache["description"] = mockWeather;
                _weatherCache["temperature"] = mockTemperature;
                _weatherCache["humidity"] = mockHumidity;
                _weatherCache["wind_speed"] = 15f;
                _weatherCache["pressure"] = 1013f;
            }
            else
            {
                // In real implementation, call weather API
                StartCoroutine(FetchWeatherFromAPI());
            }
            
            _lastUpdateTime = Time.time;
        }
        
        /// <summary>
        /// Fetch weather from external API
        /// </summary>
        private IEnumerator FetchWeatherFromAPI()
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogWarning("[WeatherExtension] No API key provided, using mock data");
                yield break;
            }
            
            // Example API call (OpenWeatherMap)
            var url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            
            using (var www = new UnityEngine.Networking.UnityWebRequest(url))
            {
                www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                
                yield return www.SendWebRequest();
                
                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = www.downloadHandler.text;
                        ParseWeatherResponse(response);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[WeatherExtension] Failed to parse weather response: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"[WeatherExtension] Weather API request failed: {www.error}");
                }
            }
        }
        
        /// <summary>
        /// Parse weather API response
        /// </summary>
        private void ParseWeatherResponse(string jsonResponse)
        {
            try
            {
                // Simple JSON parsing (in production, use proper JSON library)
                if (jsonResponse.Contains("\"main\""))
                {
                    // Extract temperature and humidity
                    var tempStart = jsonResponse.IndexOf("\"temp\":") + 7;
                    var tempEnd = jsonResponse.IndexOf(",", tempStart);
                    if (tempStart > 6 && tempEnd > tempStart)
                    {
                        var tempStr = jsonResponse.Substring(tempStart, tempEnd - tempStart);
                        if (float.TryParse(tempStr, out float temp))
                        {
                            _weatherCache["temperature"] = temp;
                        }
                    }
                    
                    var humidityStart = jsonResponse.IndexOf("\"humidity\":") + 11;
                    var humidityEnd = jsonResponse.IndexOf(",", humidityStart);
                    if (humidityStart > 10 && humidityEnd > humidityStart)
                    {
                        var humidityStr = jsonResponse.Substring(humidityStart, humidityEnd - humidityStart);
                        if (float.TryParse(humidityStr, out float humidity))
                        {
                            _weatherCache["humidity"] = humidity;
                        }
                    }
                }
                
                if (jsonResponse.Contains("\"weather\""))
                {
                    var descStart = jsonResponse.IndexOf("\"description\":\"") + 15;
                    var descEnd = jsonResponse.IndexOf("\"", descStart);
                    if (descStart > 14 && descEnd > descStart)
                    {
                        var description = jsonResponse.Substring(descStart, descEnd - descStart);
                        _weatherCache["description"] = description;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WeatherExtension] Failed to parse weather response: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set city for weather queries
        /// </summary>
        public void SetCity(string newCity)
        {
            city = newCity;
            UpdateWeatherData();
        }
        
        /// <summary>
        /// Get supported weather actions
        /// </summary>
        public string[] GetSupportedActions()
        {
            return new string[]
            {
                "GetCurrentWeather",
                "GetWeatherForecast", 
                "SetCity",
                "GetTemperature",
                "GetHumidity"
            };
        }
        
        /// <summary>
        /// Get extension capabilities
        /// </summary>
        public ExtensionCapability[] GetCapabilities()
        {
            return new ExtensionCapability[]
            {
                new ExtensionCapability("Weather Information", "Provides real-time weather data and forecasts")
                {
                    SupportedActions = GetSupportedActions(),
                    IsActive = true,
                    PerformanceScore = 0.9f
                }
            };
        }
    }
}
