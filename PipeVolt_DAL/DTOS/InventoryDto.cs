using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // Inventory
    public class InventoryDto
    {
        public int InventoryId { get; set; }
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public int? PurchaseOrderId { get; set; }
        public int Quantity { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class CreateInventoryDto
    {
        [Required] public int WarehouseId { get; set; }
        [Required] public int ProductId { get; set; }
        public int? PurchaseOrderId { get; set; }
        [Required] public int Quantity { get; set; }
    }
    public class UpdateInventoryDto
    {
        [Required] public int InventoryId { get; set; }
        [Required] public int WarehouseId { get; set; }
        [Required] public int ProductId { get; set; }
        public int? PurchaseOrderId { get; set; }
        [Required] public int Quantity { get; set; }
    }
}
