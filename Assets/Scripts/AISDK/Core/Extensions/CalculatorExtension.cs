using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Extensions
{
    /// <summary>
    /// Calculator extension providing mathematical functions to AI agents
    /// </summary>
    public class CalculatorExtension : BaseFunctionExtension
    {
        #region Properties
        public override string Name => "Calculator";
        public override string Version => "1.0.0";
        public override string Description => "Provides mathematical calculation functions";
        #endregion

        #region Inspector Settings
        [Header("Calculator Settings")]
        [SerializeField] private bool enableAdvancedMath = true;
        [SerializeField] private int maxDecimalPlaces = 6;
        [SerializeField] private bool enableStatistics = true;
        #endregion

        #region Function Initialization
        protected override void InitializeFunctions()
        {
            // Basic arithmetic
            AddBasicArithmeticFunctions();
            
            // Advanced math functions
            if (enableAdvancedMath)
            {
                AddAdvancedMathFunctions();
            }
            
            // Statistics functions
            if (enableStatistics)
            {
                AddStatisticsFunctions();
            }
            
            LogInfo($"Initialized Calculator with {_functionDefinitions.Count} functions");
        }

        private void AddBasicArithmeticFunctions()
        {
            // Add function
            var addParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["a"] = CreateParameter("number", "First number", true),
                ["b"] = CreateParameter("number", "Second number", true)
            }, new List<string> { "a", "b" });
            
            AddFunction("add", "Add two numbers", addParams, "add(5, 3) = 8");

            // Subtract function
            var subtractParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["a"] = CreateParameter("number", "Number to subtract from", true),
                ["b"] = CreateParameter("number", "Number to subtract", true)
            }, new List<string> { "a", "b" });
            
            AddFunction("subtract", "Subtract two numbers", subtractParams, "subtract(10, 3) = 7");

            // Multiply function
            var multiplyParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["a"] = CreateParameter("number", "First number", true),
                ["b"] = CreateParameter("number", "Second number", true)
            }, new List<string> { "a", "b" });
            
            AddFunction("multiply", "Multiply two numbers", multiplyParams, "multiply(4, 5) = 20");

            // Divide function
            var divideParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["a"] = CreateParameter("number", "Dividend", true),
                ["b"] = CreateParameter("number", "Divisor", true)
            }, new List<string> { "a", "b" });
            
            AddFunction("divide", "Divide two numbers", divideParams, "divide(15, 3) = 5");

            // Power function
            var powerParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["base"] = CreateParameter("number", "Base number", true),
                ["exponent"] = CreateParameter("number", "Exponent", true)
            }, new List<string> { "base", "exponent" });
            
            AddFunction("power", "Raise a number to a power", powerParams, "power(2, 3) = 8");
        }

        private void AddAdvancedMathFunctions()
        {
            // Square root
            var sqrtParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["number"] = CreateParameter("number", "Number to find square root of", true)
            }, new List<string> { "number" });
            
            AddFunction("sqrt", "Calculate square root", sqrtParams, "sqrt(16) = 4");

            // Sine function
            var sinParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["angle"] = CreateParameter("number", "Angle in degrees", true)
            }, new List<string> { "angle" });
            
            AddFunction("sin", "Calculate sine of angle in degrees", sinParams, "sin(90) = 1");

            // Cosine function
            var cosParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["angle"] = CreateParameter("number", "Angle in degrees", true)
            }, new List<string> { "angle" });
            
            AddFunction("cos", "Calculate cosine of angle in degrees", cosParams, "cos(0) = 1");

            // Logarithm function
            var logParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["number"] = CreateParameter("number", "Number to find logarithm of", true),
                ["base"] = CreateParameter("number", "Logarithm base", false, null, 10)
            }, new List<string> { "number" });
            
            AddFunction("log", "Calculate logarithm", logParams, "log(100, 10) = 2");
        }

        private void AddStatisticsFunctions()
        {
            // Average function
            var avgParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["numbers"] = CreateParameter("array", "Array of numbers", true)
            }, new List<string> { "numbers" });
            
            AddFunction("average", "Calculate average of numbers", avgParams, "average([1,2,3,4,5]) = 3");

            // Sum function
            var sumParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["numbers"] = CreateParameter("array", "Array of numbers", true)
            }, new List<string> { "numbers" });
            
            AddFunction("sum", "Calculate sum of numbers", sumParams, "sum([1,2,3,4,5]) = 15");

            // Min/Max functions
            var minMaxParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["numbers"] = CreateParameter("array", "Array of numbers", true)
            }, new List<string> { "numbers" });
            
            AddFunction("min", "Find minimum value", minMaxParams, "min([1,2,3,4,5]) = 1");
            AddFunction("max", "Find maximum value", minMaxParams, "max([1,2,3,4,5]) = 5");
        }
        #endregion

        #region Function Execution
        protected override IEnumerator ExecuteFunctionInternal(string functionName, Dictionary<string, object> arguments, Action<FunctionResult> onComplete)
        {
            try
            {
                string result = ExecuteCalculation(functionName, arguments);
                onComplete(new FunctionResult(functionName, Name, true, result));
            }
            catch (Exception ex)
            {
                LogError($"Calculation error in {functionName}: {ex.Message}");
                onComplete(new FunctionResult(functionName, Name, false, "", ex.Message));
            }
            yield break;
        }

        private string ExecuteCalculation(string functionName, Dictionary<string, object> arguments)
        {
            switch (functionName.ToLowerInvariant())
            {
                case "add":
                    return FormatResult(GetArgument<double>(arguments, "a") + GetArgument<double>(arguments, "b"));
                
                case "subtract":
                    return FormatResult(GetArgument<double>(arguments, "a") - GetArgument<double>(arguments, "b"));
                
                case "multiply":
                    return FormatResult(GetArgument<double>(arguments, "a") * GetArgument<double>(arguments, "b"));
                
                case "divide":
                    var divisor = GetArgument<double>(arguments, "b");
                    if (Math.Abs(divisor) < 1e-10)
                        throw new DivideByZeroException("Cannot divide by zero");
                    return FormatResult(GetArgument<double>(arguments, "a") / divisor);
                
                case "power":
                    return FormatResult(Math.Pow(GetArgument<double>(arguments, "base"), GetArgument<double>(arguments, "exponent")));
                
                case "sqrt":
                    var number = GetArgument<double>(arguments, "number");
                    if (number < 0)
                        throw new ArgumentException("Cannot calculate square root of negative number");
                    return FormatResult(Math.Sqrt(number));
                
                case "sin":
                    return FormatResult(Math.Sin(GetArgument<double>(arguments, "angle") * Math.PI / 180));
                
                case "cos":
                    return FormatResult(Math.Cos(GetArgument<double>(arguments, "angle") * Math.PI / 180));
                
                case "log":
                    var logNumber = GetArgument<double>(arguments, "number");
                    var logBase = GetArgument<double>(arguments, "base", 10);
                    if (logNumber <= 0)
                        throw new ArgumentException("Logarithm input must be positive");
                    return FormatResult(Math.Log(logNumber, logBase));
                
                case "average":
                    var avgNumbers = GetNumberArray(arguments, "numbers");
                    return FormatResult(avgNumbers.Average());
                
                case "sum":
                    var sumNumbers = GetNumberArray(arguments, "numbers");
                    return FormatResult(sumNumbers.Sum());
                
                case "min":
                    var minNumbers = GetNumberArray(arguments, "numbers");
                    return FormatResult(minNumbers.Min());
                
                case "max":
                    var maxNumbers = GetNumberArray(arguments, "numbers");
                    return FormatResult(maxNumbers.Max());
                
                default:
                    throw new NotSupportedException($"Function {functionName} is not supported");
            }
        }

        private double[] GetNumberArray(Dictionary<string, object> arguments, string key)
        {
            if (!arguments.ContainsKey(key))
                throw new ArgumentException($"Missing required parameter: {key}");

            var value = arguments[key];
            
            if (value is Newtonsoft.Json.Linq.JArray jArray)
            {
                return jArray.ToObject<double[]>();
            }
            
            if (value is System.Collections.IEnumerable enumerable)
            {
                var list = new List<double>();
                foreach (var item in enumerable)
                {
                    list.Add(Convert.ToDouble(item));
                }
                return list.ToArray();
            }
            
            throw new ArgumentException($"Parameter {key} must be an array of numbers");
        }

        private string FormatResult(double result)
        {
            if (double.IsNaN(result))
                return "NaN";
            if (double.IsPositiveInfinity(result))
                return "Infinity";
            if (double.IsNegativeInfinity(result))
                return "-Infinity";
            
            return Math.Round(result, maxDecimalPlaces).ToString();
        }
        #endregion

        #region Test Implementation
        protected override IEnumerator PerformTest(Action<bool> onComplete)
        {
            try
            {
                // Test basic arithmetic
                var testArgs = new Dictionary<string, object> { ["a"] = 5.0, ["b"] = 3.0 };
                var addResult = ExecuteCalculation("add", testArgs);
                
                if (addResult == "8")
                {
                    LogInfo("Calculator test passed");
                    onComplete(true);
                }
                else
                {
                    LogError($"Calculator test failed: expected 8, got {addResult}");
                    onComplete(false);
                }
            }
            catch (Exception ex)
            {
                LogError($"Calculator test error: {ex.Message}");
                onComplete(false);
            }
            yield break;
        }
        #endregion
    }
}