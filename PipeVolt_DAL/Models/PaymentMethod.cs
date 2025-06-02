using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Models
{
    [Table("PAYMENT_METHOD")]
    public partial class PaymentMethod
    {
        [Key]
        [Column("payment_method_id")]
        public int PaymentMethodId { get; set; }

        [Column("method_name")]
        [StringLength(50)]
        public string MethodName { get; set; } = null!;

        [Column("description")]
        [StringLength(255)]
        public string? Description { get; set; }

        [Column("is_online")]
        public bool IsOnline { get; set; } // Phân biệt Online hay Offline

        [InverseProperty("PaymentMethod")]
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
        [InverseProperty("PaymentMethod")]
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
}
