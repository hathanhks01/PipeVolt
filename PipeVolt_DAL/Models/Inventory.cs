using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("INVENTORY")]
public partial class Inventory
{
    [Key]
    [Column("inventory_id")]
    public int InventoryId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("purchase_order_id")]
    public int? PurchaseOrderId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Inventories")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("PurchaseOrderId")]
    [InverseProperty("Inventories")]
    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    [ForeignKey("WarehouseId")]
    [InverseProperty("Inventories")]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
