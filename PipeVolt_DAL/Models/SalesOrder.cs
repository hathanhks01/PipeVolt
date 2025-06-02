using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("SALES_ORDER")]
[Index("OrderCode", Name = "UQ__SALES_OR__99D12D3F11C3C006", IsUnique = true)]
public partial class SalesOrder
{
    [Key]
    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("order_code")]
    [StringLength(50)]
    public string? OrderCode { get; set; }

    [Column("customer_id")]
    public int? CustomerId { get; set; }

    [Column("employee_id")]
    public int? EmployeeId { get; set; }

    [Column("order_date", TypeName = "datetime")]
    public DateTime? OrderDate { get; set; }

    [Column("total_amount")]
    public double? TotalAmount { get; set; }

    [Column("discount_amount")]
    public double? DiscountAmount { get; set; }

    [Column("tax_amount")]
    public double? TaxAmount { get; set; }

    [Column("net_amount")]
    public double? NetAmount { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("SalesOrders")]
    public virtual Customer? Customer { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("SalesOrders")]
    public virtual Employee? Employee { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    [Column("payment_method_id")]
    public int? PaymentMethodId { get; set; }

    [ForeignKey("PaymentMethodId")]
    [InverseProperty("SalesOrders")]
    public virtual PaymentMethod? PaymentMethod { get; set; }
    [InverseProperty("Order")] 
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; }
    [InverseProperty("Order")]
    public virtual ICollection<Invoice>? Invoices { get; set; } = new List<Invoice>();
}
