using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // SalesOrder
    public class SalesOrderDto
    {
        public int OrderId { get; set; }
        public string? OrderCode { get; set; }
        public int? CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime? OrderDate { get; set; }
        public double? TotalAmount { get; set; }
        public double? DiscountAmount { get; set; }
        public double? TaxAmount { get; set; }
        public double? NetAmount { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
    }
    public class CreateSalesOrderDto
    {
        public string? OrderCode { get; set; }
        public int? CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public double? TotalAmount { get; set; }
        public double? DiscountAmount { get; set; }
        public double? TaxAmount { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
    }
    public class UpdateSalesOrderDto
    {
        [Required] public int OrderId { get; set; }
        public string? OrderCode { get; set; }
        public int? CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public double? TotalAmount { get; set; }
        public double? DiscountAmount { get; set; }
        public double? TaxAmount { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
    }
}
