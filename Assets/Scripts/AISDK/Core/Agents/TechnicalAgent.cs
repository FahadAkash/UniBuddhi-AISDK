using AISDK.Core.Models;

namespace AISDK.Core.Agents
{
    /// <summary>
    /// Technical agent - Precise technical information
    /// </summary>
    public class TechnicalAgent : BaseAgent
    {
        public TechnicalAgent() : base(AgentType.Technical)
        {
        }

        protected override ChatRequest CreateChatRequest(Message[] messages)
        {
            var request = base.CreateChatRequest(messages);
            
            // Technical-specific optimizations
            if (string.IsNullOrEmpty(request.SystemPrompt))
            {
                request.SystemPrompt = "You are a technical AI expert. Provide precise, detailed technical information and solutions.";
            }
            
            // Lower temperature for more precise responses
            if (request.Temperature > 0.3f)
            {
                request.Temperature = 0.3f;
            }
            
            return request;
        }
    }
}
