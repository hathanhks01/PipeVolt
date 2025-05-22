using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    public class CartDto
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
        public double TotalAmount { get; set; } // Tổng giá trị giỏ hàng
    }
    public class CreateCartDto
    {
        public int CustomerId { get; set; }
    }
    public class UpdateCartDto
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
    }
}
