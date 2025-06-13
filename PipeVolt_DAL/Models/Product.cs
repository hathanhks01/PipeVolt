using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("PRODUCT")]
[Index("ProductCode", Name = "UQ__PRODUCT__AE1A8CC4FB8207A6", IsUnique = true)]
public partial class Product
{
    [Key]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("product_code")]
    [StringLength(50)]
    public string ProductCode { get; set; } = null!;

    [Column("product_name")]
    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("brand_id")]
    public int? BrandId { get; set; }
    [Column("selling_price")]
    public double? SellingPrice { get; set; }

    [Column("unit")]
    [StringLength(50)]
    public string? Unit { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    public int? quantity { get; set; } 

    [Column("image_url")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [ForeignKey("BrandId")]
    [InverseProperty("Products")]
    public virtual Brand? Brand { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual ProductCategory? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    [InverseProperty("Product")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    [InverseProperty("Product")]
    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
    [InverseProperty("Product")]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [InverseProperty("Product")]
    public virtual ICollection<Supply> Supplies { get; set; } = new List<Supply>();

    [InverseProperty("Product")]
    public virtual ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
    [InverseProperty("Product")]
    public virtual ICollection<InvoiceDetail>? InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
