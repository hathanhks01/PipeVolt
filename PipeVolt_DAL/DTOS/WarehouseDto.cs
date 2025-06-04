using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // Warehouse
    public class WarehouseDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; }

        public string? WarehouseName { get; set; }
        public string? Address { get; set; }
    }
    public class CreateWarehouseDto
    {
        [Required] public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; }

        public string? Address { get; set; }
    }
    public class UpdateWarehouseDto
    {
        [Required] public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; }

        [Required] public string WarehouseName { get; set; } = string.Empty;
        public string? Address { get; set; }
    }
}
