using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("PURCHASE_ORDER")]
[Index("PurchaseOrderCode", Name = "UQ__PURCHASE__19DA46F1BE5EB09D", IsUnique = true)]
public partial class PurchaseOrder
{
    [Key]
    [Column("purchase_order_id")]
    public int PurchaseOrderId { get; set; }

    [Column("purchase_order_code")]
    [StringLength(50)]
    public string? PurchaseOrderCode { get; set; }

    [Column("supplier_id")]
    public int? SupplierId { get; set; }

    [Column("employee_id")]
    public int? EmployeeId { get; set; }

    [Column("order_date", TypeName = "datetime")]
    public DateTime? OrderDate { get; set; }

    [Column("total_amount")]
    public double? TotalAmount { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("PurchaseOrders")]
    public virtual Employee? Employee { get; set; }

    [InverseProperty("PurchaseOrder")]
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    [InverseProperty("PurchaseOrder")]
    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    [ForeignKey("SupplierId")]
    [InverseProperty("PurchaseOrders")]
    public virtual Supplier? Supplier { get; set; }
}
