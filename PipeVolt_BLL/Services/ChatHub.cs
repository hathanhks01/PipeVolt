using Microsoft.AspNetCore.SignalR;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
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

        public async Task SendMessage(SendMessageDto messageDto)
        {
            var message = await _chatService.SendMessageAsync(messageDto);

            // Gửi tin nhắn đến tất cả thành viên trong phòng chat
            await Clients.Group($"ChatRoom_{messageDto.ChatRoomId}")
                .SendAsync("ReceiveMessage", message);
        }

        public async Task MarkAsRead(int messageId)
        {
            await _chatService.MarkMessageAsReadAsync(messageId);

            // Thông báo tin nhắn đã được đọc
            await Clients.Group($"ChatRoom_{messageId}")
                .SendAsync("MessageRead", messageId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
