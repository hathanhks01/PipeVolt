using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.IServices
{
    public interface IChatService
    {
        Task<ChatRoomDto?> AssignEmployeeToChatAsync(int chatRoomId, int employeeId);
        Task<bool> CloseChatRoomAsync(int chatRoomId);
        Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto createDto);
        Task<List<ChatRoomDto>> GetAllChatRoomsAsync();
        Task<List<ChatMessageDto>> GetChatMessagesAsync(int chatRoomId, int page = 1, int pageSize = 50);
        Task<List<ChatRoomDto>> GetChatRoomsForCustomerAsync(int customerId);
        Task<List<ChatRoomDto>> GetChatRoomsForEmployeeAsync(int employeeId);
        Task<int> GetUnreadMessageCountAsync(int chatRoomId, int userId, int userType);
        Task MarkAllMessagesAsReadAsync(int chatRoomId, int userId, int userType);
        Task MarkMessageAsReadAsync(int messageId);
        Task<ChatMessageDto> SendMessageAsync(SendMessageDto messageDto);
    }
}