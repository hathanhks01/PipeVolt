using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Models
{
    [Table("PAYMENT_TRANSACTION")]
    public partial class PaymentTransaction
    {
        [Key]
        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("payment_method_id")]
        public int PaymentMethodId { get; set; }

        [Column("transaction_code")]
        [StringLength(100)]
        public string? TransactionCode { get; set; } 

        [Column("amount")]
        public double Amount { get; set; }

        [Column("transaction_date", TypeName = "datetime")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Column("status")]
      
        public int? Status { get; set; } 

        [Column("gateway_response")]
        public string? GatewayResponse { get; set; } // Phản hồi từ cổng thanh toán

        [ForeignKey("OrderId")]
        [InverseProperty("PaymentTransactions")]
        public virtual SalesOrder Order { get; set; } = null!;

        [ForeignKey("PaymentMethodId")]
        [InverseProperty("PaymentTransactions")]
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;
    }
}
