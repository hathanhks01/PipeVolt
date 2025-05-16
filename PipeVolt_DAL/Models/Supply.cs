using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("SUPPLY")]
public partial class Supply
{
    [Key]
    [Column("supply_id")]
    public int SupplyId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("supplier_id")]
    public int SupplierId { get; set; }

    [Column("supply_date")]
    public DateOnly SupplyDate { get; set; }

    [Column("cost_price")]
    public double CostPrice { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Supplies")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("SupplierId")]
    [InverseProperty("Supplies")]
    public virtual Supplier Supplier { get; set; } = null!;
}
