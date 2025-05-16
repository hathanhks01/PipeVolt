using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("PURCHASE_ORDER_DETAIL")]
public partial class PurchaseOrderDetail
{
    [Key]
    [Column("purchase_order_detail_id")]
    public int PurchaseOrderDetailId { get; set; }

    [Column("purchase_order_id")]
    public int PurchaseOrderId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    [Column("unit_cost")]
    public double? UnitCost { get; set; }

    [Column("line_total")]
    public double? LineTotal { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("PurchaseOrderDetails")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("PurchaseOrderId")]
    [InverseProperty("PurchaseOrderDetails")]
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}
