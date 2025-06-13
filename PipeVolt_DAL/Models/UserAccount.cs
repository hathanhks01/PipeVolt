using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

[Table("USER_ACCOUNT")]
[Index("Username", Name = "UQ__USER_ACC__F3DBC5721AE09BBF", IsUnique = true)]
public partial class UserAccount
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("username")]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = null!;

    [Column("user_type")]
    public int UserType { get; set; }

    [Column("employee_id")]
    public int? EmployeeId { get; set; }

    [Column("customer_id")]
    public int? CustomerId { get; set; }

    [Column("status")]
    public int? Status { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("UserAccounts")]
    public virtual Customer? Customer { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("UserAccount")] 
    public virtual Employee? Employee { get; set; }

}
