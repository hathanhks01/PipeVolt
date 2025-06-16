using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    public class CheckoutFullRequest
    {
        public int PaymentMethodId { get; set; }
    }

    public class 
        CheckoutPartialRequest
    {
        public int PaymentMethodId { get; set; }
        public List<int> CartItemIds { get; set; }
    }
    public class PosItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; } = 0;
    }

    // DTO for POS customer info (optional for walk-in customers)
    public class PosCustomerInfo
    {
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerTaxCode { get; set; }
    }
}
