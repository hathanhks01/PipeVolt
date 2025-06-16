using PipeVolt_DAL.Models;

namespace PipeVolt_BLL.IServices
{
    public interface IAIChatbotService
    {
        Task<string> CheckWarrantyAsync(string productCode, string serialNumber);
        Task<string> GetInstallationGuideAsync(int productId);
        Task<string> GetProductRecommendationAsync(string query, int? customerId = null);
        Task<string> GetTechnicalSupportAsync(string issue, int? productId = null);
        Task<ChatMessage> ProcessUserMessageAsync(int chatRoomId, string userMessage, int senderId, int senderType);
    }
}