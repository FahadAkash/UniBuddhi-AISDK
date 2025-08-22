using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Agents
{
    /// <summary>
    /// Analytical agent - Data analysis and problem-solving
    /// </summary>
    public class AnalyticalAgent : BaseAgent
    {
        public AnalyticalAgent() : base(AgentType.Analytical)
        {
        }

        protected override ChatRequest CreateChatRequest(Message[] messages)
        {
            var request = base.CreateChatRequest(messages);
            
            // Analytical-specific optimizations
            if (string.IsNullOrEmpty(request.SystemPrompt))
            {
                request.SystemPrompt = "You are an analytical AI focused on data analysis, logical reasoning, and problem-solving.";
            }
            
            // Lower temperature for more analytical responses
            if (request.Temperature > 0.4f)
            {
                request.Temperature = 0.4f;
            }
            
            return request;
        }
    }
}
