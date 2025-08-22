using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Extensions
{
    /// <summary>
    /// Weather information extension providing weather-related functions to AI agents
    /// </summary>
    public class WeatherInfoExtension : BaseFunctionExtension
    {
        #region Properties
        public override string Name => "WeatherInfo";
        public override string Version => "1.0.0";
        public override string Description => "Provides weather information and forecasting functions";
        #endregion

        #region Inspector Settings
        [Header("Weather Settings")]
        [SerializeField] private string defaultLocation = "New York";
        [SerializeField] private string temperatureUnit = "Celsius";
        [SerializeField] private bool includeExtendedForecast = true;
        [SerializeField] private bool simulateWeatherData = true; // For demo purposes
        #endregion

        #region Private Fields
        private readonly string[] weatherConditions = { "Sunny", "Cloudy", "Rainy", "Snowy", "Foggy", "Windy", "Stormy" };
        private readonly Dictionary<string, WeatherData> cachedWeather = new Dictionary<string, WeatherData>();
        #endregion

        #region Function Initialization
        protected override void InitializeFunctions()
        {
            // Current weather function
            var currentWeatherParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["location"] = CreateParameter("string", "Location to get weather for", false, null, defaultLocation),
                ["units"] = CreateParameter("string", "Temperature units", false, new List<string> { "Celsius", "Fahrenheit", "Kelvin" }, temperatureUnit)
            });
            
            AddFunction("get_current_weather", "Get current weather for a location", currentWeatherParams, "get_current_weather('London', 'Celsius')");

            // Weather forecast function
            var forecastParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["location"] = CreateParameter("string", "Location to get forecast for", false, null, defaultLocation),
                ["days"] = CreateParameter("number", "Number of days to forecast", false, null, 5),
                ["units"] = CreateParameter("string", "Temperature units", false, new List<string> { "Celsius", "Fahrenheit", "Kelvin" }, temperatureUnit)
            });
            
            AddFunction("get_weather_forecast", "Get weather forecast for multiple days", forecastParams, "get_weather_forecast('Tokyo', 3, 'Celsius')");

            // Weather alerts function
            var alertsParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["location"] = CreateParameter("string", "Location to check alerts for", false, null, defaultLocation)
            });
            
            AddFunction("get_weather_alerts", "Get weather alerts and warnings", alertsParams, "get_weather_alerts('Miami')");

            // Air quality function
            var airQualityParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["location"] = CreateParameter("string", "Location to check air quality for", false, null, defaultLocation)
            });
            
            AddFunction("get_air_quality", "Get air quality information", airQualityParams, "get_air_quality('Beijing')");

            // Historical weather function
            var historicalParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["location"] = CreateParameter("string", "Location for historical data", true),
                ["date"] = CreateParameter("string", "Date in YYYY-MM-DD format", true),
                ["units"] = CreateParameter("string", "Temperature units", false, new List<string> { "Celsius", "Fahrenheit", "Kelvin" }, temperatureUnit)
            }, new List<string> { "location", "date" });
            
            AddFunction("get_historical_weather", "Get historical weather data for a specific date", historicalParams, "get_historical_weather('Paris', '2023-12-25', 'Celsius')");
            
            LogInfo($"Initialized Weather extension with {_functionDefinitions.Count} functions");
        }
        #endregion

        #region Function Execution
        protected override IEnumerator ExecuteFunctionInternal(string functionName, Dictionary<string, object> arguments, Action<FunctionResult> onComplete)
        {
            try
            {
                string result = ExecuteWeatherFunction(functionName, arguments);
                onComplete(new FunctionResult(functionName, Name, true, result));
            }
            catch (Exception ex)
            {
                LogError($"Weather function error in {functionName}: {ex.Message}");
                onComplete(new FunctionResult(functionName, Name, false, "", ex.Message));
            }
            yield break;
        }

        private string ExecuteWeatherFunction(string functionName, Dictionary<string, object> arguments)
        {
            switch (functionName.ToLowerInvariant())
            {
                case "get_current_weather":
                    return GetCurrentWeather(arguments);
                
                case "get_weather_forecast":
                    return GetWeatherForecast(arguments);
                
                case "get_weather_alerts":
                    return GetWeatherAlerts(arguments);
                
                case "get_air_quality":
                    return GetAirQuality(arguments);
                
                case "get_historical_weather":
                    return GetHistoricalWeather(arguments);
                
                default:
                    throw new NotSupportedException($"Weather function {functionName} is not supported");
            }
        }

        private string GetCurrentWeather(Dictionary<string, object> arguments)
        {
            var location = GetArgument<string>(arguments, "location", defaultLocation);
            var units = GetArgument<string>(arguments, "units", temperatureUnit);
            
            var weather = simulateWeatherData ? GenerateSimulatedWeather(location) : GetRealWeatherData(location);
            
            var temperature = ConvertTemperature(weather.Temperature, "Celsius", units);
            
            return $"Current weather in {location}:\n" +
                   $"Temperature: {temperature:F1}°{GetTemperatureSymbol(units)}\n" +
                   $"Condition: {weather.Condition}\n" +
                   $"Humidity: {weather.Humidity}%\n" +
                   $"Wind Speed: {weather.WindSpeed} km/h\n" +
                   $"Visibility: {weather.Visibility} km\n" +
                   $"Last Updated: {weather.LastUpdated:yyyy-MM-dd HH:mm}";
        }

        private string GetWeatherForecast(Dictionary<string, object> arguments)
        {
            var location = GetArgument<string>(arguments, "location", defaultLocation);
            var days = Math.Min(GetArgument<int>(arguments, "days", 5), 7); // Limit to 7 days
            var units = GetArgument<string>(arguments, "units", temperatureUnit);
            
            var forecast = new List<string>();
            forecast.Add($"Weather forecast for {location} ({days} days):");
            
            for (int i = 0; i < days; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var weather = simulateWeatherData ? GenerateSimulatedWeather(location, i) : GetRealWeatherData(location, i);
                var temperature = ConvertTemperature(weather.Temperature, "Celsius", units);
                
                forecast.Add($"{date:MMM dd}: {weather.Condition}, {temperature:F1}°{GetTemperatureSymbol(units)}, Humidity: {weather.Humidity}%");
            }
            
            return string.Join("\n", forecast);
        }

        private string GetWeatherAlerts(Dictionary<string, object> arguments)
        {
            var location = GetArgument<string>(arguments, "location", defaultLocation);
            
            // Simulate weather alerts
            var alerts = new List<string>();
            
            var random = new System.Random(location.GetHashCode() + DateTime.Now.Day);
            if (random.Next(0, 100) < 30) // 30% chance of alerts
            {
                var alertTypes = new[] { "High Wind Warning", "Heavy Rain Advisory", "Heat Advisory", "Fog Advisory", "Storm Watch" };
                var alertType = alertTypes[random.Next(alertTypes.Length)];
                alerts.Add($"⚠️ {alertType} for {location}");
                alerts.Add($"Issued: {DateTime.Now:yyyy-MM-dd HH:mm}");
                alerts.Add("Please take appropriate precautions.");
            }
            else
            {
                alerts.Add($"No weather alerts currently active for {location}.");
            }
            
            return string.Join("\n", alerts);
        }

        private string GetAirQuality(Dictionary<string, object> arguments)
        {
            var location = GetArgument<string>(arguments, "location", defaultLocation);
            
            // Simulate air quality data
            var random = new System.Random(location.GetHashCode());
            var aqi = random.Next(50, 150);
            var quality = GetAirQualityDescription(aqi);
            var pollutants = new[] { "PM2.5", "PM10", "O3", "NO2", "SO2", "CO" };
            
            var result = $"Air Quality in {location}:\n" +
                        $"AQI: {aqi} ({quality})\n" +
                        $"Primary Pollutant: {pollutants[random.Next(pollutants.Length)]}\n" +
                        $"Last Updated: {DateTime.Now:yyyy-MM-dd HH:mm}";
            
            return result;
        }

        private string GetHistoricalWeather(Dictionary<string, object> arguments)
        {
            var location = GetArgument<string>(arguments, "location");
            var dateStr = GetArgument<string>(arguments, "date");
            var units = GetArgument<string>(arguments, "units", temperatureUnit);
            
            if (!DateTime.TryParse(dateStr, out DateTime date))
            {
                throw new ArgumentException("Invalid date format. Use YYYY-MM-DD.");
            }
            
            if (date > DateTime.Now)
            {
                throw new ArgumentException("Cannot get historical data for future dates.");
            }
            
            // Simulate historical weather
            var weather = GenerateSimulatedWeather(location, date.DayOfYear);
            var temperature = ConvertTemperature(weather.Temperature, "Celsius", units);
            
            return $"Historical weather for {location} on {date:yyyy-MM-dd}:\n" +
                   $"Temperature: {temperature:F1}°{GetTemperatureSymbol(units)}\n" +
                   $"Condition: {weather.Condition}\n" +
                   $"Humidity: {weather.Humidity}%\n" +
                   $"Wind Speed: {weather.WindSpeed} km/h";
        }
        #endregion

        #region Helper Methods
        private WeatherData GenerateSimulatedWeather(string location, int dayOffset = 0)
        {
            var random = new System.Random(location.GetHashCode() + dayOffset);
            
            return new WeatherData
            {
                Location = location,
                Temperature = random.Next(-10, 35) + random.NextDouble(), // -10 to 35°C
                Condition = weatherConditions[random.Next(weatherConditions.Length)],
                Humidity = random.Next(30, 90),
                WindSpeed = random.Next(5, 30),
                Visibility = random.Next(5, 20),
                LastUpdated = DateTime.Now
            };
        }

        private WeatherData GetRealWeatherData(string location, int dayOffset = 0)
        {
            // In a real implementation, this would call a weather API
            // For now, return simulated data
            return GenerateSimulatedWeather(location, dayOffset);
        }

        private double ConvertTemperature(double celsius, string fromUnit, string toUnit)
        {
            if (fromUnit == toUnit) return celsius;
            
            switch (toUnit.ToLowerInvariant())
            {
                case "fahrenheit":
                    return celsius * 9 / 5 + 32;
                case "kelvin":
                    return celsius + 273.15;
                default:
                    return celsius; // Default to Celsius
            }
        }

        private string GetTemperatureSymbol(string unit)
        {
            switch (unit.ToLowerInvariant())
            {
                case "fahrenheit": return "F";
                case "kelvin": return "K";
                default: return "C";
            }
        }

        private string GetAirQualityDescription(int aqi)
        {
            if (aqi <= 50) return "Good";
            if (aqi <= 100) return "Moderate";
            if (aqi <= 150) return "Unhealthy for Sensitive Groups";
            if (aqi <= 200) return "Unhealthy";
            if (aqi <= 300) return "Very Unhealthy";
            return "Hazardous";
        }
        #endregion

        #region Test Implementation
        protected override IEnumerator PerformTest(Action<bool> onComplete)
        {
            try
            {
                var testArgs = new Dictionary<string, object> { ["location"] = "Test City" };
                var weatherResult = GetCurrentWeather(testArgs);
                
                if (weatherResult.Contains("Test City") && weatherResult.Contains("Temperature:"))
                {
                    LogInfo("Weather extension test passed");
                    onComplete(true);
                }
                else
                {
                    LogError("Weather extension test failed");
                    onComplete(false);
                }
            }
            catch (Exception ex)
            {
                LogError($"Weather extension test error: {ex.Message}");
                onComplete(false);
            }
            yield break;
        }
        #endregion

        #region Weather Data Class
        private class WeatherData
        {
            public string Location { get; set; }
            public double Temperature { get; set; }
            public string Condition { get; set; }
            public int Humidity { get; set; }
            public int WindSpeed { get; set; }
            public int Visibility { get; set; }
            public DateTime LastUpdated { get; set; }
        }
        #endregion
    }
}