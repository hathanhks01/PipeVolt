using Microsoft.EntityFrameworkCore;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PipeVolt_DAL.Common.DataType;

namespace PipeVolt_BLL.Services
{
    public class ChatService : IChatService
    {
        private readonly PipeVoltDbContext _context;
        private readonly ILoggerService _logger;

        public ChatService(PipeVoltDbContext context, ILoggerService logger)
        {
            _context = context;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto createDto)
        {
            _logger.LogInformation($"[CreateChatRoomAsync] Creating chat room for customer {createDto.CustomerId}, employee {createDto.EmployeeId}");

            try
            {
                var existingRoom = await _context.ChatRooms
                    .FirstOrDefaultAsync(r => r.CustomerId == createDto.CustomerId &&
                                             r.Status == 1);

                if (existingRoom != null)
                {
                    _logger.LogInformation($"[CreateChatRoomAsync] Existing chat room found for customer {createDto.CustomerId}: {existingRoom.ChatRoomId}");
                    return await MapToChatRoomDto(existingRoom);
                }

                var chatRoom = new ChatRoom
                {
                    CustomerId = createDto.CustomerId,
                    EmployeeId = createDto.EmployeeId,
                    RoomName = createDto.RoomName ?? $"Chat với khách hàng #{createDto.CustomerId}",
                    Status = 1
                };

                _context.ChatRooms.Add(chatRoom);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[CreateChatRoomAsync] Created new chat room {chatRoom.ChatRoomId} for customer {createDto.CustomerId}");

                return await MapToChatRoomDto(chatRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError("[CreateChatRoomAsync] Error creating chat room", ex);
                throw;
            }
        }

        public async Task<List<ChatRoomDto>> GetChatRoomsForCustomerAsync(int customerId)
        {
            _logger.LogInformation($"[GetChatRoomsForCustomerAsync] Fetching chat rooms for customer {customerId}");
            try
            {
                var rooms = await _context.ChatRooms
                    .Include(r => r.Customer)
                    .Include(r => r.Employee)
                    .Include(r => r.ChatMessages)
                    .Where(r => r.CustomerId == customerId)
                    .OrderByDescending(r => r.LastMessageAt ?? r.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"[GetChatRoomsForCustomerAsync] Found {rooms.Count} chat rooms for customer {customerId}");

                var result = new List<ChatRoomDto>();
                foreach (var room in rooms)
                {
                    result.Add(await MapToChatRoomDto(room));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("[GetChatRoomsForCustomerAsync] Error fetching chat rooms", ex);
                throw;
            }
        }

        public async Task<List<ChatRoomDto>> GetChatRoomsForEmployeeAsync(int employeeId)
        {
            _logger.LogInformation($"[GetChatRoomsForEmployeeAsync] Fetching chat rooms for employee {employeeId}");
            try
            {
                var rooms = await _context.ChatRooms
                    .Include(r => r.Customer)
                    .Include(r => r.Employee)
                    .Include(r => r.ChatMessages)
                    .Where(r => r.EmployeeId == employeeId || r.EmployeeId == null)
                    .OrderByDescending(r => r.LastMessageAt ?? r.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"[GetChatRoomsForEmployeeAsync] Found {rooms.Count} chat rooms for employee {employeeId}");

                var result = new List<ChatRoomDto>();
                foreach (var room in rooms)
                {
                    result.Add(await MapToChatRoomDto(room));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("[GetChatRoomsForEmployeeAsync] Error fetching chat rooms", ex);
                throw;
            }
        }

        public async Task<List<ChatRoomDto>> GetAllChatRoomsAsync()
        {
            _logger.LogInformation("[GetAllChatRoomsAsync] Fetching all chat rooms");
            try
            {
                var rooms = await _context.ChatRooms
                    .Include(r => r.Customer)
                    .Include(r => r.Employee)
                    .Include(r => r.ChatMessages)
                    .OrderByDescending(r => r.LastMessageAt ?? r.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"[GetAllChatRoomsAsync] Found {rooms.Count} chat rooms");

                var result = new List<ChatRoomDto>();
                foreach (var room in rooms)
                {
                    result.Add(await MapToChatRoomDto(room));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("[GetAllChatRoomsAsync] Error fetching chat rooms", ex);
                throw;
            }
        }

        public async Task<ChatMessageDto> SendMessageAsync(SendMessageDto messageDto)
        {
            _logger.LogInformation($"[SendMessageAsync] Sending message to chatRoomId {messageDto.ChatRoomId} by sender {messageDto.SenderId} (type {messageDto.SenderType})");
            try
            {
                var message = new ChatMessage
                {
                    ChatRoomId = messageDto.ChatRoomId,
                    SenderId = messageDto.SenderId,
                    SenderType = messageDto.SenderType,
                    MessageContent = messageDto.MessageContent,
                    MessageType = messageDto.MessageType,
                    AttachmentUrl = messageDto.AttachmentUrl
                };

                _context.ChatMessages.Add(message);

                // Cập nhật thời gian tin nhắn cuối cùng của phòng chat
                var chatRoom = await _context.ChatRooms.FindAsync(messageDto.ChatRoomId);
                if (chatRoom != null)
                {
                    chatRoom.LastMessageAt = DateTime.Now;
                    chatRoom.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"[SendMessageAsync] Message sent with id {message.MessageId} to chatRoomId {messageDto.ChatRoomId}");

                return await MapToChatMessageDto(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("[SendMessageAsync] Error sending message", ex);
                throw;
            }
        }

        public async Task<List<ChatMessageDto>> GetChatMessagesAsync(int chatRoomId, int page = 1, int pageSize = 50)
        {
            _logger.LogInformation($"[GetChatMessagesAsync] Fetching messages for chatRoomId {chatRoomId}, page {page}, pageSize {pageSize}");
            try
            {
                var messages = await _context.ChatMessages
                    .Where(m => m.ChatRoomId == chatRoomId)
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation($"[GetChatMessagesAsync] Found {messages.Count} messages for chatRoomId {chatRoomId}");

                var result = new List<ChatMessageDto>();
                foreach (var message in messages.OrderBy(m => m.SentAt))
                {
                    result.Add(await MapToChatMessageDto(message));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("[GetChatMessagesAsync] Error fetching messages", ex);
                throw;
            }
        }

        public async Task MarkMessageAsReadAsync(int messageId)
        {
            _logger.LogInformation($"[MarkMessageAsReadAsync] Marking message {messageId} as read");
            try
            {
                var message = await _context.ChatMessages.FindAsync(messageId);
                if (message != null && !message.IsRead)
                {
                    message.IsRead = true;
                    message.ReadAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"[MarkMessageAsReadAsync] Message {messageId} marked as read");
                }
                else
                {
                    _logger.LogWarning($"[MarkMessageAsReadAsync] Message {messageId} not found or already read");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[MarkMessageAsReadAsync] Error marking message as read", ex);
                throw;
            }
        }

        public async Task MarkAllMessagesAsReadAsync(int chatRoomId, int userId, int userType)
        {
            _logger.LogInformation($"[MarkAllMessagesAsReadAsync] Marking all messages as read in chatRoomId {chatRoomId} for userId {userId}, userType {userType}");
            try
            {
                var messages = await _context.ChatMessages
                    .Where(m => m.ChatRoomId == chatRoomId &&
                               !m.IsRead &&
                               !(m.SenderId == userId && m.SenderType == userType))
                    .ToListAsync();

                foreach (var message in messages)
                {
                    message.IsRead = true;
                    message.ReadAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"[MarkAllMessagesAsReadAsync] Marked {messages.Count} messages as read in chatRoomId {chatRoomId}");
            }
            catch (Exception ex)
            {
                _logger.LogError("[MarkAllMessagesAsReadAsync] Error marking all messages as read", ex);
                throw;
            }
        }

        public async Task<int> GetUnreadMessageCountAsync(int chatRoomId, int userId, int userType)
        {
            _logger.LogInformation($"[GetUnreadMessageCountAsync] Counting unread messages in chatRoomId {chatRoomId} for userId {userId}, userType {userType}");
            try
            {
                var count = await _context.ChatMessages
                    .CountAsync(m => m.ChatRoomId == chatRoomId &&
                                   !m.IsRead &&
                                   !(m.SenderId == userId && m.SenderType == userType));
                _logger.LogInformation($"[GetUnreadMessageCountAsync] Found {count} unread messages in chatRoomId {chatRoomId}");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError("[GetUnreadMessageCountAsync] Error counting unread messages", ex);
                throw;
            }
        }

        public async Task<ChatRoomDto?> AssignEmployeeToChatAsync(int chatRoomId, int employeeId)
        {
            _logger.LogInformation($"[AssignEmployeeToChatAsync] Assigning employee {employeeId} to chatRoomId {chatRoomId}");
            try
            {
                var chatRoom = await _context.ChatRooms.FindAsync(chatRoomId);
                if (chatRoom == null)
                {
                    _logger.LogWarning($"[AssignEmployeeToChatAsync] Chat room {chatRoomId} not found");
                    return null;
                }

                chatRoom.EmployeeId = employeeId;
                chatRoom.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"[AssignEmployeeToChatAsync] Assigned employee {employeeId} to chatRoomId {chatRoomId}");

                return await MapToChatRoomDto(chatRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError("[AssignEmployeeToChatAsync] Error assigning employee to chat room", ex);
                throw;
            }
        }

        public async Task<bool> CloseChatRoomAsync(int chatRoomId)
        {
            _logger.LogInformation($"[CloseChatRoomAsync] Closing chat room {chatRoomId}");
            try
            {
                var chatRoom = await _context.ChatRooms.FindAsync(chatRoomId);
                if (chatRoom == null)
                {
                    _logger.LogWarning($"[CloseChatRoomAsync] Chat room {chatRoomId} not found");
                    return false;
                }

                chatRoom.Status = (int)ChatRoomStatus.Closed;
                chatRoom.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"[CloseChatRoomAsync] Closed chat room {chatRoomId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("[CloseChatRoomAsync] Error closing chat room", ex);
                throw;
            }
        }

        // Helper methods
        private async Task<ChatRoomDto> MapToChatRoomDto(ChatRoom room)
        {
            // Không cần log cho helper nội bộ
            var customer = await _context.Customers.FindAsync(room.CustomerId);
            var employee = room.EmployeeId.HasValue ?
                await _context.Employees.FindAsync(room.EmployeeId.Value) : null;

            var lastMessage = await _context.ChatMessages
                .Where(m => m.ChatRoomId == room.ChatRoomId)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            var unreadCount = await _context.ChatMessages
                .CountAsync(m => m.ChatRoomId == room.ChatRoomId && !m.IsRead);

            return new ChatRoomDto
            {
                ChatRoomId = room.ChatRoomId,
                CustomerId = room.CustomerId,
                CustomerName = customer?.CustomerName ?? "Unknown",
                EmployeeId = room.EmployeeId,
                EmployeeName = employee?.EmployeeName,
                RoomName = room.RoomName,
                Status = room.Status,
                CreatedAt = room.CreatedAt,
                LastMessageAt = room.LastMessageAt,
                LastMessage = lastMessage?.MessageContent,
                UnreadCount = unreadCount
            };
        }

        private async Task<ChatMessageDto> MapToChatMessageDto(ChatMessage message)
        {
            // Không cần log cho helper nội bộ
            string senderName = "";
            if (message.SenderType == (int)UserType.Customer)
            {
                var customer = await _context.Customers.FindAsync(message.SenderId);
                senderName = customer?.CustomerName ?? "Unknown Customer";
            }
            else if (message.SenderType == (int)UserType.Employee)
            {
                var employee = await _context.Employees.FindAsync(message.SenderId);
                senderName = employee?.EmployeeName ?? "Unknown Employee";
            }

            return new ChatMessageDto
            {
                MessageId = message.MessageId,
                ChatRoomId = message.ChatRoomId,
                SenderId = message.SenderId,
                SenderType = (int)message.SenderType,
                SenderName = senderName,
                MessageContent = message.MessageContent,
                MessageType = message.MessageType,
                AttachmentUrl = message.AttachmentUrl,
                IsRead = message.IsRead,
                SentAt = message.SentAt,
                ReadAt = message.ReadAt
            };
        }
    }
}