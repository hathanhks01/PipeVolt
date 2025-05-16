using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // PurchaseOrderDetail
    public class PurchaseOrderDetailDto
    {
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        public int? Quantity { get; set; }
        public double? UnitCost { get; set; }
        public double? LineTotal { get; set; }
    }
    public class CreatePurchaseOrderDetailDto
    {
        [Required] public int PurchaseOrderId { get; set; }
        [Required] public int ProductId { get; set; }
        public int? Quantity { get; set; }
        public double? UnitCost { get; set; }
    }
    public class UpdatePurchaseOrderDetailDto
    {
        [Required] public int PurchaseOrderDetailId { get; set; }
        [Required] public int PurchaseOrderId { get; set; }
        [Required] public int ProductId { get; set; }
        public int? Quantity { get; set; }
        public double? UnitCost { get; set; }
    }
}
