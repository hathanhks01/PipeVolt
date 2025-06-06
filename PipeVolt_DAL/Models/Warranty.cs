using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("WARRANTY")]
public partial class Warranty
{
    [Key]
    [Column("warranty_id")]
    public int WarrantyId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("serial_number")]
    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("status")]
    public int? Status { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Warranties")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("Warranties")]
    public virtual Product Product { get; set; } = null!;
}
