using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // Supplier
    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ContactPerson { get; set; }
    }
    public class CreateSupplierDto
    {
        [Required] public string SupplierName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ContactPerson { get; set; }
    }
    public class UpdateSupplierDto
    {
        [Required] public int SupplierId { get; set; }
        [Required] public string SupplierName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ContactPerson { get; set; }
    }
}
