using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    public class ChatRoomDto
    {
        public int ChatRoomId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? RoomName { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ChatMessageDto
    {
        public int MessageId { get; set; }
        public int ChatRoomId { get; set; }
        public int SenderId { get; set; }
        public string SenderType { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string MessageContent { get; set; } = null!;
        public string MessageType { get; set; } = null!;
        public string? AttachmentUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class SendMessageDto
    {
        public int ChatRoomId { get; set; }
        public int SenderId { get; set; }
        public string SenderType { get; set; } = null!;
        public string MessageContent { get; set; } = null!;
        public string MessageType { get; set; } = "Text";
        public string? AttachmentUrl { get; set; }
    }

    public class CreateChatRoomDto
    {
        public int CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public string? RoomName { get; set; }
    }
}
