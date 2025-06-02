using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Models
{
    [Table("INVOICE")]
    [Index("InvoiceNumber", Name = "UQ__INVOICE__number", IsUnique = true)]
    public partial class Invoice
    {
        [Key]
        [Column("invoice_id")]
        public int InvoiceId { get; set; }

        [Column("invoice_number")]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = null!; // Số hóa đơn duy nhất

        [Column("invoice_template")]
        [StringLength(20)]
        public string? InvoiceTemplate { get; set; } // Mẫu số hóa đơn (VD: 01GTKT)

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("employee_id")]
        public int? EmployeeId { get; set; }

        [Column("issue_date", TypeName = "datetime")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Column("due_date", TypeName = "datetime")]
        public DateTime? DueDate { get; set; }

        // Thông tin thuế
        [Column("subtotal")]
        public double SubTotal { get; set; } // Tiền hàng chưa thuế

        [Column("vat_rate")]
        public double VatRate { get; set; } = 0.1; // Thuế VAT (10%)

        [Column("vat_amount")]
        public double VatAmount { get; set; } // Tiền thuế VAT

        [Column("total_amount")]
        public double TotalAmount { get; set; } // Tổng tiền sau thuế

        [Column("discount_amount")]
        public double? DiscountAmount { get; set; }

        // Thông tin khách hàng trên hóa đơn (snapshot)
        [Column("customer_name")]
        [StringLength(150)]
        public string CustomerName { get; set; } = null!;

        [Column("customer_address")]
        [StringLength(255)]
        public string? CustomerAddress { get; set; }

        [Column("customer_tax_code")]
        [StringLength(20)]
        public string? CustomerTaxCode { get; set; } // Mã số thuế khách hàng

        [Column("customer_phone")]
        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        // Trạng thái hóa đơn
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Issued, Paid, Cancelled

        [Column("payment_status")]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Unpaid"; // Paid, Unpaid, Partial

        // Ghi chú
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at", TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("OrderId")]
        [InverseProperty("Invoices")]
        public virtual SalesOrder Order { get; set; } = null!;

        [ForeignKey("CustomerId")]
        [InverseProperty("Invoices")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("EmployeeId")]
        [InverseProperty("Invoices")]
        public virtual Employee? Employee { get; set; }

        [InverseProperty("Invoice")]
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
    }

}
