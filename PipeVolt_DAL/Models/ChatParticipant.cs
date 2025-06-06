using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Models
{
    [Table("CHAT_PARTICIPANT")]
    public partial class ChatParticipant
    {
        [Key]
        [Column("participant_id")]
        public int ParticipantId { get; set; }

        [Column("chat_room_id")]
        public int ChatRoomId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("user_type")]
        [StringLength(20)]
        public int UserType { get; set; } // Customer, Employee

        [Column("joined_at", TypeName = "datetime")]
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        [Column("left_at", TypeName = "datetime")]
        public DateTime? LeftAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [ForeignKey("ChatRoomId")]
        [InverseProperty("ChatParticipants")]
        public virtual ChatRoom ChatRoom { get; set; } = null!;
    }
}
