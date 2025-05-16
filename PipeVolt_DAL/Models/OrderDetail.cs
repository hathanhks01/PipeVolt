using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("ORDER_DETAIL")]
public partial class OrderDetail
{
    [Key]
    [Column("order_detail_id")]
    public int OrderDetailId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    [Column("unit_price")]
    public double? UnitPrice { get; set; }

    [Column("discount")]
    public double? Discount { get; set; }

    [Column("line_total")]
    public double? LineTotal { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderDetails")]
    public virtual SalesOrder Order { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("OrderDetails")]
    public virtual Product Product { get; set; } = null!;
}
