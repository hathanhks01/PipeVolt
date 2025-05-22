using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Models
{
    [Table("CART")]
    public partial class Cart
    {
        [Key]
        [Column("cart_id")]
        public int CartId { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at", TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CustomerId")]
        [InverseProperty("Carts")]
        public virtual Customer Customer { get; set; } = null!;

        [InverseProperty("Cart")]
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
