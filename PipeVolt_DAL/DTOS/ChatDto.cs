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
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public bool? EmployeeOnline { get; set; } = false;
    }

    public class ChatMessageDto
    {
        public int MessageId { get; set; }
        public int ChatRoomId { get; set; }
        public int SenderId { get; set; }
        public int SenderType { get; set; } // 1 = Customer, 2 = Employee
        public string SenderName { get; set; } = null!;
        public string MessageContent { get; set; } = null!;
        public int MessageType { get; set; } // 0 = Text, 1 = Product, 2 = Link, 3 = Image
        public string? AttachmentUrl { get; set; }
        public ProductMessageData? ProductData { get; set; } // For MessageType = 1
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class ProductMessageData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public string? ProductCode { get; set; }
    }

    public class SendMessageDto
    {
        public int ChatRoomId { get; set; }
        public int SenderId { get; set; }
        public int SenderType { get; set; } // 1 = Customer, 2 = Employee
        public string MessageContent { get; set; } = null!;
        public int MessageType { get; set; } = 0; // 0 = Text, 1 = Product, 2 = Link, 3 = Image
        public string? AttachmentUrl { get; set; }
        public ProductMessageData? ProductData { get; set; } // For MessageType = 1
    }

    public class CreateChatRoomDto
    {
        public int CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public string? RoomName { get; set; }
    }
}
