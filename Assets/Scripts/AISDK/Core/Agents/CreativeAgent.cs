using AISDK.Core.Models;

namespace AISDK.Core.Agents
{
    /// <summary>
    /// Creative agent - Imaginative and artistic responses
    /// </summary>
    public class CreativeAgent : BaseAgent
    {
        public CreativeAgent() : base(AgentType.Creative)
        {
        }

        protected override ChatRequest CreateChatRequest(Message[] messages)
        {
            var request = base.CreateChatRequest(messages);
            
            // Creative-specific optimizations
            if (string.IsNullOrEmpty(request.SystemPrompt))
            {
                request.SystemPrompt = "You are a creative AI focused on imagination, storytelling, and artistic expression. Be creative and engaging.";
            }
            
            // Higher temperature for more creative responses
            if (request.Temperature < 0.8f)
            {
                request.Temperature = 0.8f;
            }
            
            return request;
        }
    }
}
