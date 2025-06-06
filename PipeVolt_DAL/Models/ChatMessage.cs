using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Models
{
    [Table("CHAT_MESSAGE")]
    public partial class ChatMessage
    {
        [Key]
        [Column("message_id")]
        public int MessageId { get; set; }

        [Column("chat_room_id")]
        public int ChatRoomId { get; set; }

        [Column("sender_id")]
        public int SenderId { get; set; }

        [Column("sender_type")]
        [StringLength(20)]
        public int? SenderType { get; set; }  // Customer, Employee

        [Column("message_content")]
        public string MessageContent { get; set; } = null!;

        [Column("message_type")]
        [StringLength(20)]
        public int MessageType { get; set; }  // Text, Image, File, System

        [Column("attachment_url")]
        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("sent_at", TypeName = "datetime")]
        public DateTime SentAt { get; set; } = DateTime.Now;

        [Column("read_at", TypeName = "datetime")]
        public DateTime? ReadAt { get; set; }

        [ForeignKey("ChatRoomId")]
        [InverseProperty("ChatMessages")]
        public virtual ChatRoom ChatRoom { get; set; } = null!;
    }
}
