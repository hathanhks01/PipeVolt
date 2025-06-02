using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("CUSTOMER")]
[Index("CustomerCode", Name = "UQ__CUSTOMER__6A9E4CB785B4A11F", IsUnique = true)]
public partial class Customer
{
    [Key]
    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("customer_code")]
    [StringLength(50)]
    public string? CustomerCode { get; set; }

    [Column("customer_name")]
    [StringLength(150)]
    public string? CustomerName { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string? Address { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Column("registration_date")]
    public DateOnly? RegistrationDate { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    [InverseProperty("Customer")]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [InverseProperty("Customer")]
    public virtual ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();

    [InverseProperty("Customer")]
    public virtual ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
    [InverseProperty("Customer")]
    public virtual ICollection<Invoice>? Invoices { get; set; } = new List<Invoice>();
}
