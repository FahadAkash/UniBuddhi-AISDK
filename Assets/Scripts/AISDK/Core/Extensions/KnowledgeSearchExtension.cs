using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Extensions
{
    /// <summary>
    /// Knowledge search extension providing information lookup functions to AI agents
    /// </summary>
    public class KnowledgeSearchExtension : BaseFunctionExtension
    {
        #region Properties
        public override string Name => "KnowledgeSearch";
        public override string Version => "1.0.0";
        public override string Description => "Provides knowledge search and information lookup functions";
        #endregion

        #region Inspector Settings
        [Header("Knowledge Search Settings")]
        [SerializeField] private bool enableFactLookup = true;
        [SerializeField] private bool enableDefinitions = true;
        [SerializeField] private bool enableCalculations = true;
        [SerializeField] private int maxSearchResults = 5;
        #endregion

        #region Private Fields
        private Dictionary<string, KnowledgeEntry> knowledgeBase;
        #endregion

        #region Function Initialization
        protected override void InitializeFunctions()
        {
            InitializeKnowledgeBase();
            
            // Search facts function
            if (enableFactLookup)
            {
                var searchFactsParams = CreateParameters(new Dictionary<string, FunctionProperty>
                {
                    ["query"] = CreateParameter("string", "Search query for facts", true),
                    ["category"] = CreateParameter("string", "Category to search in", false, 
                        new List<string> { "science", "history", "geography", "technology", "general" }, "general"),
                    ["max_results"] = CreateParameter("number", "Maximum number of results", false, null, maxSearchResults)
                }, new List<string> { "query" });
                
                AddFunction("search_facts", "Search for factual information", searchFactsParams, "search_facts('solar system', 'science', 3)");
            }

            // Get definition function
            if (enableDefinitions)
            {
                var definitionParams = CreateParameters(new Dictionary<string, FunctionProperty>
                {
                    ["term"] = CreateParameter("string", "Term to define", true),
                    ["detailed"] = CreateParameter("boolean", "Return detailed definition", false, null, false)
                }, new List<string> { "term" });
                
                AddFunction("get_definition", "Get definition of a term", definitionParams, "get_definition('artificial intelligence', true)");
            }

            // Search by category function
            var categoryParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["category"] = CreateParameter("string", "Category to browse", true, 
                    new List<string> { "science", "history", "geography", "technology", "general" }),
                ["limit"] = CreateParameter("number", "Number of entries to return", false, null, 5)
            }, new List<string> { "category" });
            
            AddFunction("browse_category", "Browse knowledge by category", categoryParams, "browse_category('science', 3)");

            // Get random fact function
            var randomFactParams = CreateParameters(new Dictionary<string, FunctionProperty>
            {
                ["category"] = CreateParameter("string", "Category for random fact", false, 
                    new List<string> { "science", "history", "geography", "technology", "general", "any" }, "any")
            });
            
            AddFunction("get_random_fact", "Get a random interesting fact", randomFactParams, "get_random_fact('science')");

            // Unit conversion function
            if (enableCalculations)
            {
                var conversionParams = CreateParameters(new Dictionary<string, FunctionProperty>
                {
                    ["value"] = CreateParameter("number", "Value to convert", true),
                    ["from_unit"] = CreateParameter("string", "Unit to convert from", true),
                    ["to_unit"] = CreateParameter("string", "Unit to convert to", true),
                    ["unit_type"] = CreateParameter("string", "Type of unit", false, 
                        new List<string> { "length", "weight", "temperature", "volume", "area" }, "length")
                }, new List<string> { "value", "from_unit", "to_unit" });
                
                AddFunction("convert_units", "Convert between different units", conversionParams, "convert_units(5, 'feet', 'meters', 'length')");
            }
            
            LogInfo($"Initialized Knowledge Search with {_functionDefinitions.Count} functions and {knowledgeBase.Count} knowledge entries");
        }

        private void InitializeKnowledgeBase()
        {
            knowledgeBase = new Dictionary<string, KnowledgeEntry>();
            
            // Science facts
            AddKnowledgeEntry("photosynthesis", "Science", 
                "Photosynthesis", 
                "The process by which plants use sunlight, water, and carbon dioxide to produce oxygen and energy in the form of sugar.",
                "Photosynthesis is a complex biochemical process that occurs in the chloroplasts of plant cells. It consists of two main stages: the light-dependent reactions and the Calvin cycle. This process is essential for life on Earth as it produces oxygen and forms the base of most food chains.");

            AddKnowledgeEntry("gravity", "Science",
                "Gravity",
                "A fundamental force that attracts objects with mass toward each other.",
                "Gravity is one of the four fundamental forces in physics. Described by Einstein's theory of general relativity as the curvature of spacetime caused by mass and energy. On Earth, gravity accelerates objects at approximately 9.8 meters per second squared.");

            AddKnowledgeEntry("dna", "Science",
                "DNA (Deoxyribonucleic Acid)",
                "The hereditary material in humans and almost all other organisms that carries genetic instructions.",
                "DNA is a double helix structure composed of nucleotides containing four bases: adenine (A), thymine (T), guanine (G), and cytosine (C). The sequence of these bases determines genetic information. DNA is found in the nucleus of cells and is responsible for heredity and protein synthesis.");

            // History facts
            AddKnowledgeEntry("ancient rome", "History",
                "Ancient Rome",
                "An ancient civilization that began as a city-state in Italy and became one of the largest empires in history.",
                "Ancient Rome lasted from 753 BC to 476 AD (Western Empire) and 1453 AD (Eastern/Byzantine Empire). It was known for its military prowess, engineering achievements like aqueducts and roads, legal system, and cultural influence that still affects the modern world.");

            AddKnowledgeEntry("industrial revolution", "History",
                "Industrial Revolution",
                "The transition to new manufacturing processes in Europe and the United States from about 1760 to 1840.",
                "The Industrial Revolution marked the shift from hand production methods to machines, new chemical manufacturing processes, and the use of steam power. It began in Britain and spread worldwide, fundamentally changing economic and social structures.");

            // Geography facts
            AddKnowledgeEntry("mount everest", "Geography",
                "Mount Everest",
                "The Earth's highest mountain above sea level, located in the Himalayas on the border between Nepal and Tibet.",
                "Mount Everest stands at 8,848.86 meters (29,031.7 feet) above sea level. Known as Sagarmatha in Nepali and Chomolungma in Tibetan, it was first successfully climbed by Sir Edmund Hillary and Tenzing Norgay in 1953.");

            AddKnowledgeEntry("amazon rainforest", "Geography",
                "Amazon Rainforest",
                "The world's largest tropical rainforest, covering much of the Amazon Basin in South America.",
                "The Amazon rainforest covers approximately 5.5 million square kilometers and spans across nine countries, with about 60% in Brazil. It's often called the 'lungs of the Earth' due to its role in oxygen production and carbon dioxide absorption.");

            // Technology facts
            AddKnowledgeEntry("artificial intelligence", "Technology",
                "Artificial Intelligence (AI)",
                "The simulation of human intelligence in machines that are programmed to think and learn.",
                "AI encompasses various technologies including machine learning, deep learning, natural language processing, and computer vision. It aims to create systems that can perform tasks typically requiring human intelligence, such as visual perception, speech recognition, and decision-making.");

            AddKnowledgeEntry("quantum computing", "Technology",
                "Quantum Computing",
                "A type of computation that harnesses quantum mechanical phenomena to process information.",
                "Quantum computers use quantum bits (qubits) instead of classical bits, allowing them to exist in multiple states simultaneously through superposition. This enables them to solve certain problems exponentially faster than classical computers, particularly in cryptography and optimization.");
        }

        private void AddKnowledgeEntry(string key, string category, string title, string shortDescription, string detailedDescription)
        {
            knowledgeBase[key.ToLowerInvariant()] = new KnowledgeEntry
            {
                Key = key,
                Category = category,
                Title = title,
                ShortDescription = shortDescription,
                DetailedDescription = detailedDescription,
                Tags = title.ToLowerInvariant().Split(' ').ToList()
            };
        }
        #endregion

        #region Function Execution
        protected override IEnumerator ExecuteFunctionInternal(string functionName, Dictionary<string, object> arguments, Action<FunctionResult> onComplete)
        {
            try
            {
                string result = ExecuteKnowledgeFunction(functionName, arguments);
                onComplete(new FunctionResult(functionName, Name, true, result));
            }
            catch (Exception ex)
            {
                LogError($"Knowledge search error in {functionName}: {ex.Message}");
                onComplete(new FunctionResult(functionName, Name, false, "", ex.Message));
            }
            yield break;
        }

        private string ExecuteKnowledgeFunction(string functionName, Dictionary<string, object> arguments)
        {
            switch (functionName.ToLowerInvariant())
            {
                case "search_facts":
                    return SearchFacts(arguments);
                
                case "get_definition":
                    return GetDefinition(arguments);
                
                case "browse_category":
                    return BrowseCategory(arguments);
                
                case "get_random_fact":
                    return GetRandomFact(arguments);
                
                case "convert_units":
                    return ConvertUnits(arguments);
                
                default:
                    throw new NotSupportedException($"Knowledge function {functionName} is not supported");
            }
        }

        private string SearchFacts(Dictionary<string, object> arguments)
        {
            var query = GetArgument<string>(arguments, "query").ToLowerInvariant();
            var category = GetArgument<string>(arguments, "category", "general").ToLowerInvariant();
            var maxResults = GetArgument<int>(arguments, "max_results", this.maxSearchResults);

            var results = knowledgeBase.Values
                .Where(entry => 
                    (category == "general" || entry.Category.ToLowerInvariant() == category) &&
                    (entry.Title.ToLowerInvariant().Contains(query) || 
                     entry.ShortDescription.ToLowerInvariant().Contains(query) ||
                     entry.Tags.Any(tag => tag.Contains(query))))
                .Take(maxResults)
                .ToList();

            if (!results.Any())
            {
                return $"No facts found for query '{query}' in category '{category}'.";
            }

            var response = $"Found {results.Count} fact(s) for '{query}':\n\n";
            for (int i = 0; i < results.Count; i++)
            {
                var entry = results[i];
                response += $"{i + 1}. **{entry.Title}** ({entry.Category})\n";
                response += $"   {entry.ShortDescription}\n\n";
            }

            return response.TrimEnd();
        }

        private string GetDefinition(Dictionary<string, object> arguments)
        {
            var term = GetArgument<string>(arguments, "term").ToLowerInvariant();
            var detailed = GetArgument<bool>(arguments, "detailed", false);

            var entry = knowledgeBase.Values
                .FirstOrDefault(e => e.Title.ToLowerInvariant().Contains(term) || e.Key.Contains(term));

            if (entry == null)
            {
                return $"No definition found for '{term}'. Try searching with a different term or check the spelling.";
            }

            var response = $"**{entry.Title}**\n\n";
            response += detailed ? entry.DetailedDescription : entry.ShortDescription;

            return response;
        }

        private string BrowseCategory(Dictionary<string, object> arguments)
        {
            var category = GetArgument<string>(arguments, "category").ToLowerInvariant();
            var limit = GetArgument<int>(arguments, "limit", 5);

            var entries = knowledgeBase.Values
                .Where(entry => entry.Category.ToLowerInvariant() == category)
                .Take(limit)
                .ToList();

            if (!entries.Any())
            {
                return $"No entries found in category '{category}'.";
            }

            var response = $"Knowledge entries in '{category}' category:\n\n";
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                response += $"{i + 1}. **{entry.Title}**\n";
                response += $"   {entry.ShortDescription}\n\n";
            }

            return response.TrimEnd();
        }

        private string GetRandomFact(Dictionary<string, object> arguments)
        {
            var category = GetArgument<string>(arguments, "category", "any").ToLowerInvariant();

            var entries = knowledgeBase.Values.ToList();
            if (category != "any")
            {
                entries = entries.Where(entry => entry.Category.ToLowerInvariant() == category).ToList();
            }

            if (!entries.Any())
            {
                return $"No facts available in category '{category}'.";
            }

            var random = new System.Random();
            var randomEntry = entries[random.Next(entries.Count)];

            return $"**Random Fact: {randomEntry.Title}** ({randomEntry.Category})\n\n{randomEntry.DetailedDescription}";
        }

        private string ConvertUnits(Dictionary<string, object> arguments)
        {
            var value = GetArgument<double>(arguments, "value");
            var fromUnit = GetArgument<string>(arguments, "from_unit").ToLowerInvariant();
            var toUnit = GetArgument<string>(arguments, "to_unit").ToLowerInvariant();
            var unitType = GetArgument<string>(arguments, "unit_type", "length").ToLowerInvariant();

            double result;
            string resultUnit;

            switch (unitType)
            {
                case "length":
                    result = ConvertLength(value, fromUnit, toUnit);
                    resultUnit = toUnit;
                    break;
                case "weight":
                    result = ConvertWeight(value, fromUnit, toUnit);
                    resultUnit = toUnit;
                    break;
                case "temperature":
                    result = ConvertTemperature(value, fromUnit, toUnit);
                    resultUnit = toUnit;
                    break;
                default:
                    throw new NotSupportedException($"Unit type '{unitType}' is not supported");
            }

            return $"{value} {fromUnit} = {result:F3} {resultUnit}";
        }

        private double ConvertLength(double value, string from, string to)
        {
            // Convert to meters first
            double meters = from switch
            {
                "mm" or "millimeter" or "millimeters" => value / 1000.0,
                "cm" or "centimeter" or "centimeters" => value / 100.0,
                "m" or "meter" or "meters" => value,
                "km" or "kilometer" or "kilometers" => value * 1000.0,
                "in" or "inch" or "inches" => value * 0.0254,
                "ft" or "foot" or "feet" => value * 0.3048,
                "yd" or "yard" or "yards" => value * 0.9144,
                "mi" or "mile" or "miles" => value * 1609.344,
                _ => throw new ArgumentException($"Unknown length unit: {from}")
            };

            // Convert from meters to target unit
            return to switch
            {
                "mm" or "millimeter" or "millimeters" => meters * 1000.0,
                "cm" or "centimeter" or "centimeters" => meters * 100.0,
                "m" or "meter" or "meters" => meters,
                "km" or "kilometer" or "kilometers" => meters / 1000.0,
                "in" or "inch" or "inches" => meters / 0.0254,
                "ft" or "foot" or "feet" => meters / 0.3048,
                "yd" or "yard" or "yards" => meters / 0.9144,
                "mi" or "mile" or "miles" => meters / 1609.344,
                _ => throw new ArgumentException($"Unknown length unit: {to}")
            };
        }

        private double ConvertWeight(double value, string from, string to)
        {
            // Convert to grams first
            double grams = from switch
            {
                "mg" or "milligram" or "milligrams" => value / 1000.0,
                "g" or "gram" or "grams" => value,
                "kg" or "kilogram" or "kilograms" => value * 1000.0,
                "oz" or "ounce" or "ounces" => value * 28.3495,
                "lb" or "pound" or "pounds" => value * 453.592,
                _ => throw new ArgumentException($"Unknown weight unit: {from}")
            };

            // Convert from grams to target unit
            return to switch
            {
                "mg" or "milligram" or "milligrams" => grams * 1000.0,
                "g" or "gram" or "grams" => grams,
                "kg" or "kilogram" or "kilograms" => grams / 1000.0,
                "oz" or "ounce" or "ounces" => grams / 28.3495,
                "lb" or "pound" or "pounds" => grams / 453.592,
                _ => throw new ArgumentException($"Unknown weight unit: {to}")
            };
        }

        private double ConvertTemperature(double value, string from, string to)
        {
            // Convert to Celsius first
            double celsius = from switch
            {
                "c" or "celsius" => value,
                "f" or "fahrenheit" => (value - 32) * 5.0 / 9.0,
                "k" or "kelvin" => value - 273.15,
                _ => throw new ArgumentException($"Unknown temperature unit: {from}")
            };

            // Convert from Celsius to target unit
            return to switch
            {
                "c" or "celsius" => celsius,
                "f" or "fahrenheit" => celsius * 9.0 / 5.0 + 32,
                "k" or "kelvin" => celsius + 273.15,
                _ => throw new ArgumentException($"Unknown temperature unit: {to}")
            };
        }
        #endregion

        #region Test Implementation
        protected override IEnumerator PerformTest(Action<bool> onComplete)
        {
            try
            {
                var testArgs = new Dictionary<string, object> { ["query"] = "gravity" };
                var searchResult = SearchFacts(testArgs);
                
                if (searchResult.Contains("gravity") || searchResult.Contains("Gravity"))
                {
                    LogInfo("Knowledge search test passed");
                    onComplete(true);
                }
                else
                {
                    LogError("Knowledge search test failed");
                    onComplete(false);
                }
            }
            catch (Exception ex)
            {
                LogError($"Knowledge search test error: {ex.Message}");
                onComplete(false);
            }
            yield break;
        }
        #endregion

        #region Knowledge Entry Class
        private class KnowledgeEntry
        {
            public string Key { get; set; }
            public string Category { get; set; }
            public string Title { get; set; }
            public string ShortDescription { get; set; }
            public string DetailedDescription { get; set; }
            public List<string> Tags { get; set; }
        }
        #endregion
    }
}