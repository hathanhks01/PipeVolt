using Microsoft.AspNetCore.SignalR;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        // Key: "userId_userType", Value: ConnectionId
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();
        // Key: ConnectionId, Value: "userId_userType"
        private static readonly ConcurrentDictionary<string, string> ConnectionUsers = new();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Client tham gia phòng chat để nhận tin nhắn realtime
        /// </summary>
        public async Task JoinChatRoom(int chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ChatRoom_{chatRoomId}");
        }

        /// <summary>
        /// Client rời phòng chat
        /// </summary>
        public async Task LeaveChatRoom(int chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ChatRoom_{chatRoomId}");
        }

        /// <summary>
        /// Đăng ký user để nhận notification targeted
        /// userType: 1=Employee, 2=Customer (theo DataType.SenderType)
        /// </summary>
        public async Task RegisterUser(int userId, int userType)
        {
            var key = $"{userId}_{userType}";
            
            // Remove old connection if exists
            if (UserConnections.TryGetValue(key, out var oldConnId))
            {
                ConnectionUsers.TryRemove(oldConnId, out _);
            }
            
            UserConnections[key] = Context.ConnectionId;
            ConnectionUsers[Context.ConnectionId] = key;

            // Employee (senderType=1) join group để nhận notification danh sách chat
            if (userType == (int)DataType.SenderType.Employee)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Employee_{userId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, "AllEmployees");
            }
        }

        /// <summary>
        /// Gửi tin nhắn qua SignalR (dùng khi muốn gửi trực tiếp qua hub, 
        /// thông thường nên gửi qua REST API để có persistence)
        /// </summary>
        public async Task SendMessage(SendMessageDto messageDto)
        {
            var message = await _chatService.SendMessageAsync(messageDto);

            // Broadcast tin nhắn đến tất cả trong phòng
            await Clients.Group($"ChatRoom_{messageDto.ChatRoomId}")
                .SendAsync("ReceiveMessage", message);

            // Clear typing indicator
            await Clients.Group($"ChatRoom_{messageDto.ChatRoomId}")
                .SendAsync("UserStoppedTyping", messageDto.SenderId);

            // Nếu Customer gửi → notify cho tất cả Employee
            if (messageDto.SenderType == (int)DataType.SenderType.Customer)
            {
                await NotifyEmployeesChatUpdated(messageDto.ChatRoomId);
            }
        }

        /// <summary>
        /// Đánh dấu tin nhắn đã đọc
        /// </summary>
        public async Task MarkAsRead(int messageId, int chatRoomId)
        {
            await _chatService.MarkMessageAsReadAsync(messageId);

            await Clients.Group($"ChatRoom_{chatRoomId}")
                .SendAsync("MessageRead", messageId);
        }

        /// <summary>
        /// Notify danh sách chat của employee cần cập nhật
        /// </summary>
        private async Task NotifyEmployeesChatUpdated(int chatRoomId)
        {
            try
            {
                var room = await _chatService.GetChatRoomByIdAsync(chatRoomId);
                
                // Notify employee được assign
                if (room?.EmployeeId.HasValue == true)
                {
                    await Clients.Group($"Employee_{room.EmployeeId.Value}")
                        .SendAsync("ChatListUpdated", new { chatRoomId, hasNewMessage = true });
                }
                
                // Notify tất cả employee (để handle unassigned rooms)
                await Clients.Group("AllEmployees")
                    .SendAsync("ChatListUpdated", new { chatRoomId, hasNewMessage = true });
            }
            catch (Exception ex)
            {
                // Log but don't throw - notification failure shouldn't break message sending
                Console.WriteLine($"Error notifying employees: {ex.Message}");
            }
        }

        /// <summary>
        /// Typing indicator - người dùng đang gõ
        /// </summary>
        public async Task UserTyping(int chatRoomId, int senderId, string senderName)
        {
            // Broadcast đến tất cả trong phòng TRỪ người gửi
            await Clients.OthersInGroup($"ChatRoom_{chatRoomId}")
                .SendAsync("UserTyping", new 
                { 
                    senderId, 
                    senderName, 
                    chatRoomId,
                    timestamp = DateTime.UtcNow 
                });
        }

        /// <summary>
        /// Người dùng ngừng gõ
        /// </summary>
        public async Task UserStoppedTyping(int chatRoomId, int senderId)
        {
            await Clients.OthersInGroup($"ChatRoom_{chatRoomId}")
                .SendAsync("UserStoppedTyping", senderId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Cleanup connection tracking
            if (ConnectionUsers.TryRemove(Context.ConnectionId, out var userKey))
            {
                UserConnections.TryRemove(userKey, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
