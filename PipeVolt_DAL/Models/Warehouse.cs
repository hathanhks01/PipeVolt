using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("WAREHOUSE")]
public partial class Warehouse
{
    [Key]
    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("warehouse_name")]
    [StringLength(100)]
    public string? WarehouseName { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string? Address { get; set; }

    [InverseProperty("Warehouse")]
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
