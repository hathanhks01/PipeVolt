using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // PurchaseOrder
    public class PurchaseOrderDto
    {
        public int PurchaseOrderId { get; set; }
        public string? PurchaseOrderCode { get; set; }
        public int? SupplierId { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime? OrderDate { get; set; }
        public double? TotalAmount { get; set; }
        public string? Status { get; set; }
    }
    public class CreatePurchaseOrderDto
    {
        public string? PurchaseOrderCode { get; set; }
        public int? SupplierId { get; set; }
        public int? EmployeeId { get; set; }
        public double? TotalAmount { get; set; }
        public string? Status { get; set; }
    }
    public class UpdatePurchaseOrderDto
    {
        [Required] public int PurchaseOrderId { get; set; }
        public string? PurchaseOrderCode { get; set; }
        public int? SupplierId { get; set; }
        public int? EmployeeId { get; set; }
        public double? TotalAmount { get; set; }
        public string? Status { get; set; }
    }
}
