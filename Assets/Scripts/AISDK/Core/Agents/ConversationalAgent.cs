using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.Agents
{
    /// <summary>
    /// Conversational agent - Friendly, engaging conversations
    /// </summary>
    public class ConversationalAgent : BaseAgent
    {
        public ConversationalAgent() : base(AgentType.Conversational)
        {
        }

        protected override ChatRequest CreateChatRequest(Message[] messages)
        {
            var request = base.CreateChatRequest(messages);
            
            // Conversational-specific optimizations
            if (string.IsNullOrEmpty(request.SystemPrompt))
            {
                request.SystemPrompt = "You are a friendly conversational AI. Be engaging, empathetic, and personable in your responses.";
            }
            
            // Higher temperature for more engaging responses
            if (request.Temperature < 0.7f)
            {
                request.Temperature = 0.7f;
            }
            
            return request;
        }
    }
}
