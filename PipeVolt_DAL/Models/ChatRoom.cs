using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Models
{
    [Table("CHAT_ROOM")]
    public partial class ChatRoom
    {
        [Key]
        [Column("chat_room_id")]
        public int ChatRoomId { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("employee_id")]
        public int? EmployeeId { get; set; }

        [Column("room_name")]
        [StringLength(200)]
        public string? RoomName { get; set; }

        [Column("status")]
        [StringLength(20)]
        public int Status { get; set; }  // Active, Closed, Pending

        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at", TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column("last_message_at", TypeName = "datetime")]
        public DateTime? LastMessageAt { get; set; }

        [ForeignKey("CustomerId")]
        [InverseProperty("ChatRooms")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("EmployeeId")]
        [InverseProperty("ChatRooms")]
        public virtual Employee? Employee { get; set; }

        [InverseProperty("ChatRoom")]
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
        [InverseProperty("ChatRoom")]
        public virtual ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
    }
}
