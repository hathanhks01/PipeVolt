using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // Supply
    public class SupplyDto
    {
        public int SupplyId { get; set; }
        public int ProductId { get; set; }
        public int SupplierId { get; set; }
        public DateOnly SupplyDate { get; set; }
        public double CostPrice { get; set; }
    }
    public class CreateSupplyDto
    {
        [Required] public int ProductId { get; set; }
        [Required] public int SupplierId { get; set; }
        [Required] public DateOnly SupplyDate { get; set; }
        [Required] public double CostPrice { get; set; }
    }
    public class UpdateSupplyDto
    {
        [Required] public int SupplyId { get; set; }
        [Required] public int ProductId { get; set; }
        [Required] public int SupplierId { get; set; }
        [Required] public DateOnly SupplyDate { get; set; }
        [Required] public double CostPrice { get; set; }
    }
}
