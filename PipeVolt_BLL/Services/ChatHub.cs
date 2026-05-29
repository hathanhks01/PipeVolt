using Microsoft.AspNetCore.SignalR;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PipeVolt_BLL.Services
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        // Track user connections: (UserId, UserType) -> ConnectionId
        private static readonly Dictionary<string, string> UserConnections = new();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task JoinChatRoom(int chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ChatRoom_{chatRoomId}");
        }

        public async Task LeaveChatRoom(int chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ChatRoom_{chatRoomId}");
        }

        // Register user for targeted notifications
        public async Task RegisterUser(int userId, int userType)
        {
            var key = $"{userId}_{userType}";
            UserConnections[key] = Context.ConnectionId;
            
            // If employee, add to admin chat list group
            if (userType == (int)DataType.UserType.Employee)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"AdminChatList_{userId}");
            }
        }

        public async Task SendMessage(SendMessageDto messageDto)
        {
            var message = await _chatService.SendMessageAsync(messageDto);

            // Gửi tin nhắn đến tất cả thành viên trong phòng chat
            await Clients.Group($"ChatRoom_{messageDto.ChatRoomId}")
                .SendAsync("ReceiveMessage", message);
            
            // Clear typing indicator khi gửi message
            await Clients.Group($"ChatRoom_{messageDto.ChatRoomId}")
                .SendAsync("UserStoppedTyping", messageDto.SenderId);

            // If room was reopened (customer sending to previously closed room), notify admin
            if (messageDto.SenderType == (int)DataType.UserType.Customer)
            {
                await NotifyAdminChatListUpdated(messageDto.ChatRoomId);
            }
        }

        public async Task MarkAsRead(int messageId, int chatRoomId)
        {
            await _chatService.MarkMessageAsReadAsync(messageId);

            // Thông báo tin nhắn đã được đọc
            await Clients.Group($"ChatRoom_{chatRoomId}")
                .SendAsync("MessageRead", messageId);
        }

        // Thông báo admin chat list cần cập nhật
        private async Task NotifyAdminChatListUpdated(int chatRoomId)
        {
            // Get room details to find employee
            var room = await _chatService.GetChatRoomByIdAsync(chatRoomId);
            if (room?.EmployeeId.HasValue == true)
            {
                await Clients.Group($"AdminChatList_{room.EmployeeId}")
                    .SendAsync("ChatListUpdated", chatRoomId);
            }
            
            // Also notify all employees (for unassigned rooms)
            await Clients.Group("AdminChatListAll")
                .SendAsync("ChatListUpdated", chatRoomId);
        }

        // Typing indicator - gửi thông báo người dùng đang gõ
        public async Task UserTyping(int chatRoomId, string userName)
        {
            await Clients.Group($"ChatRoom_{chatRoomId}")
                .SendAsync("UserTyping", new { userName, timestamp = DateTime.Now });
        }

        // Người dùng ngừng gõ
        public async Task UserStoppedTyping(int chatRoomId, int userId)
        {
            await Clients.Group($"ChatRoom_{chatRoomId}")
                .SendAsync("UserStoppedTyping", userId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Remove user from UserConnections
            var keyToRemove = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (keyToRemove != null)
            {
                UserConnections.Remove(keyToRemove);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}
