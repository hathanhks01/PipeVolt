using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // Warranty
    public class WarrantyDto
    {
        public int WarrantyId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string? SerialNumber { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
    public class CreateWarrantyDto
    {
        [Required] public int ProductId { get; set; }
        [Required] public int CustomerId { get; set; }
        public string? SerialNumber { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
    public class UpdateWarrantyDto
    {
        [Required] public int WarrantyId { get; set; }
        [Required] public int ProductId { get; set; }
        [Required] public int CustomerId { get; set; }
        public string? SerialNumber { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
