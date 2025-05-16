using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("BRAND")]
[Index("BrandName", Name = "UQ__BRAND__0C0C3B583015C4E1", IsUnique = true)]
public partial class Brand
{
    [Key]
    [Column("brand_id")]
    public int BrandId { get; set; }

    [Column("brand_name")]
    [StringLength(100)]
    public string BrandName { get; set; } = null!;

    [InverseProperty("Brand")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
