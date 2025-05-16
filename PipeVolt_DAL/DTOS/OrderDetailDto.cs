using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // OrderDetail
    public class OrderDetailDto
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int? Quantity { get; set; }
        public double? UnitPrice { get; set; }
        public double? Discount { get; set; }
        public double? LineTotal { get; set; }
    }
    public class CreateOrderDetailDto
    {
        [Required] public int OrderId { get; set; }
        [Required] public int ProductId { get; set; }
        public int? Quantity { get; set; }
        public double? UnitPrice { get; set; }
        public double? Discount { get; set; }
    }
    public class UpdateOrderDetailDto
    {
        [Required] public int OrderDetailId { get; set; }
        [Required] public int OrderId { get; set; }
        [Required] public int ProductId { get; set; }
        public int? Quantity { get; set; }
        public double? UnitPrice { get; set; }
        public double? Discount { get; set; }
    }
}
