using PipeVolt_DAL.Models;

namespace PipeVolt_BLL.IServices
{
    public interface IAIChatbotService
    {
        Task<string> GetProductRecommendationAsync(string query, int? customerId = null);
        Task<ChatMessage> ProcessUserMessageAsync(int chatRoomId, string userMessage, int senderId, int senderType);
    }
}