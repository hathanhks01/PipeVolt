using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("EMPLOYEE")]
[Index("EmployeeCode", Name = "UQ__EMPLOYEE__B0AA7345A2B15837", IsUnique = true)]
public partial class Employee
{
    [Key]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Column("employee_code")]
    [StringLength(50)]
    public string? EmployeeCode { get; set; }

    [Column("employee_name")]
    [StringLength(150)]
    public string? EmployeeName { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Column("position")]
    [StringLength(50)]
    public string? Position { get; set; }

    [Column("hire_date")]
    public DateOnly? HireDate { get; set; }

    [InverseProperty("Employee")]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("Employee")]
    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    [InverseProperty("Employee")]
    public virtual UserAccount? UserAccount { get; set; }

    [InverseProperty("Employee")]
    public virtual ICollection<Invoice>? Invoices { get; set; } = new List<Invoice>();
    [InverseProperty("Employee")]
    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
}
