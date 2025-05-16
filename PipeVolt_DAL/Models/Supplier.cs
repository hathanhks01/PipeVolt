using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("SUPPLIER")]
public partial class Supplier
{
    [Key]
    [Column("supplier_id")]
    public int SupplierId { get; set; }

    [Column("supplier_name")]
    [StringLength(150)]
    public string? SupplierName { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string? Address { get; set; }

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("contact_person")]
    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [InverseProperty("Supplier")]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("Supplier")]
    public virtual ICollection<Supply> Supplies { get; set; } = new List<Supply>();
}
