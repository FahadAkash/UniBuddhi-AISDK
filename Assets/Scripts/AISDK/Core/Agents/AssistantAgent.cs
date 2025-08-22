using AISDK.Core.Models;

namespace AISDK.Core.Agents
{
    /// <summary>
    /// Assistant agent - General helpful AI
    /// </summary>
    public class AssistantAgent : BaseAgent
    {
        public AssistantAgent() : base(AgentType.Assistant)
        {
        }

        protected override ChatRequest CreateChatRequest(Message[] messages)
        {
            var request = base.CreateChatRequest(messages);
            
            // Assistant-specific optimizations
            if (string.IsNullOrEmpty(request.SystemPrompt))
            {
                request.SystemPrompt = "You are a helpful AI assistant. Provide clear, accurate, and helpful responses.";
            }
            
            return request;
        }
    }
}
