using System;
using System.Collections.Generic;
using UnityEngine;
using AISDK.Core.Interfaces;
using AISDK.Core.Models;
using AISDK.Core.Agents;

namespace AISDK.Core.Factories
{
    /// <summary>
    /// Factory for creating AI agents
    /// </summary>
    public static class AgentFactory
    {
        private static readonly Dictionary<AgentType, Type> _agentTypes = new Dictionary<AgentType, Type>();

        static AgentFactory()
        {
            RegisterAgents();
        }

        /// <summary>
        /// Register all available agent types
        /// </summary>
        private static void RegisterAgents()
        {
            // Register built-in agents
            RegisterAgent(AgentType.Assistant, typeof(AssistantAgent));
            RegisterAgent(AgentType.Creative, typeof(CreativeAgent));
            RegisterAgent(AgentType.Technical, typeof(TechnicalAgent));
            RegisterAgent(AgentType.Analytical, typeof(AnalyticalAgent));
            RegisterAgent(AgentType.Conversational, typeof(ConversationalAgent));
        }

        /// <summary>
        /// Register an agent type
        /// </summary>
        public static void RegisterAgent(AgentType agentType, Type agentClass)
        {
            if (typeof(IAgent).IsAssignableFrom(agentClass))
            {
                _agentTypes[agentType] = agentClass;
                Debug.Log($"[AISDK] Registered agent: {agentType} -> {agentClass.Name}");
            }
            else
            {
                Debug.LogError($"[AISDK] Failed to register agent {agentType}: {agentClass.Name} does not implement IAgent");
            }
        }

        /// <summary>
        /// Create an agent instance
        /// </summary>
        public static IAgent CreateAgent(AgentConfig config, IAIProvider provider)
        {
            try
            {
                if (!_agentTypes.ContainsKey(config.Type))
                {
                    Debug.LogError($"[AISDK] Agent type {config.Type} not registered");
                    return null;
                }

                var agentClass = _agentTypes[config.Type];
                var agent = (IAgent)Activator.CreateInstance(agentClass);

                if (agent != null)
                {
                    agent.Initialize(config, provider);
                    Debug.Log($"[AISDK] Created agent: {config.Type}");
                }

                return agent;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AISDK] Failed to create agent {config.Type}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create an agent with default configuration
        /// </summary>
        public static IAgent CreateAgent(AgentType agentType, IAIProvider provider, string systemPrompt = "", float temperature = 0.7f, int maxTokens = 1000)
        {
            var config = new AgentConfig(agentType, systemPrompt, temperature, maxTokens);
            return CreateAgent(config, provider);
        }

        /// <summary>
        /// Get all registered agent types
        /// </summary>
        public static List<AgentType> GetRegisteredAgents()
        {
            return new List<AgentType>(_agentTypes.Keys);
        }

        /// <summary>
        /// Check if an agent type is registered
        /// </summary>
        public static bool IsAgentRegistered(AgentType agentType)
        {
            return _agentTypes.ContainsKey(agentType);
        }

        /// <summary>
        /// Get agent class type
        /// </summary>
        public static Type GetAgentClass(AgentType agentType)
        {
            return _agentTypes.ContainsKey(agentType) ? _agentTypes[agentType] : null;
        }

        /// <summary>
        /// Get default system prompt for agent type
        /// </summary>
        public static string GetDefaultSystemPrompt(AgentType agentType)
        {
            switch (agentType)
            {
                case AgentType.Assistant:
                    return "You are a helpful AI assistant. Provide clear, accurate, and helpful responses.";
                case AgentType.Creative:
                    return "You are a creative AI focused on imagination, storytelling, and artistic expression. Be creative and engaging.";
                case AgentType.Technical:
                    return "You are a technical AI expert. Provide precise, detailed technical information and solutions.";
                case AgentType.Analytical:
                    return "You are an analytical AI focused on data analysis, logical reasoning, and problem-solving.";
                case AgentType.Conversational:
                    return "You are a friendly conversational AI. Be engaging, empathetic, and personable in your responses.";
                default:
                    return "You are a helpful AI assistant.";
            }
        }

        /// <summary>
        /// Get default configuration for agent type
        /// </summary>
        public static AgentConfig GetDefaultConfig(AgentType agentType)
        {
            switch (agentType)
            {
                case AgentType.Assistant:
                    return new AgentConfig(agentType, GetDefaultSystemPrompt(agentType), 0.7f, 1000);
                case AgentType.Creative:
                    return new AgentConfig(agentType, GetDefaultSystemPrompt(agentType), 0.9f, 1500);
                case AgentType.Technical:
                    return new AgentConfig(agentType, GetDefaultSystemPrompt(agentType), 0.3f, 2000);
                case AgentType.Analytical:
                    return new AgentConfig(agentType, GetDefaultSystemPrompt(agentType), 0.4f, 1500);
                case AgentType.Conversational:
                    return new AgentConfig(agentType, GetDefaultSystemPrompt(agentType), 0.8f, 1000);
                default:
                    return new AgentConfig(agentType, GetDefaultSystemPrompt(agentType), 0.7f, 1000);
            }
        }
    }
}
