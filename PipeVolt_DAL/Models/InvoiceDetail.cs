using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Models
{
    [Table("INVOICE_DETAIL")]
    public partial class InvoiceDetail
    {
        [Key]
        [Column("invoice_detail_id")]
        public int InvoiceDetailId { get; set; }

        [Column("invoice_id")]
        public int InvoiceId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("product_name")]
        [StringLength(200)]
        public string ProductName { get; set; } = null!; // Snapshot tên sản phẩm

        [Column("product_code")]
        [StringLength(50)]
        public string ProductCode { get; set; } = null!; // Snapshot mã sản phẩm

        [Column("unit")]
        [StringLength(50)]
        public string? Unit { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public double UnitPrice { get; set; }

        [Column("discount")]
        public double? Discount { get; set; }

        [Column("line_total")]
        public double? LineTotal { get; set; } // Computed: (quantity * unit_price) - discount

        // Navigation Properties
        [ForeignKey("InvoiceId")]
        [InverseProperty("InvoiceDetails")]
        public virtual Invoice Invoice { get; set; } = null!;

        [ForeignKey("ProductId")]
        [InverseProperty("InvoiceDetails")]
        public virtual Product Product { get; set; } = null!;
    }
}
