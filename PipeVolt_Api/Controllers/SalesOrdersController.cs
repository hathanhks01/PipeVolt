using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrdersController : ControllerBase
    {
        private readonly ISalesOrderService _service;   
        public SalesOrdersController(ISalesOrderService service) => _service = service;

        [HttpGet]
        [Route("GetList")]
        public async Task<ActionResult<List<SalesOrderDto>>> GetAll() => Ok(await _service.GetAllSalesOrdersAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesOrderDto>> Get(int id)
        {
            try { return Ok(await _service.GetSalesOrderByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public async Task<ActionResult<SalesOrderDto>> Create(CreateSalesOrderDto dto)
        {
            var created = await _service.AddSalesOrderAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.OrderId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSalesOrderDto dto)
        {
            if (id != dto.OrderId) return BadRequest("ID mismatch");
            var result = await _service.UpdateSalesOrderAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteSalesOrderAsync(id);
            return NoContent();
        }

        [HttpPost("Checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            if (dto == null || dto.Items == null || !dto.Items.Any())
                return BadRequest("Checkout items cannot be empty.");

            await _service.Checkout(dto);
            return Ok(new { message = "Checkout successful" });
        }

        /// <summary>
        /// Lấy danh sách đơn hàng theo userId
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Danh sách SalesOrderDto</returns>
        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<List<SalesOrderDto>>> GetSalesOrdersByUserId(int userId)
        {
            var orders = await _service.GetSalesOrdersByUserIdAsync(userId);
            if (orders == null || orders.Count == 0)
                return NotFound("Không tìm thấy đơn hàng cho user này.");
            return Ok(orders);
        }

        [HttpGet("print-bill/{orderId}")]
        public async Task<ActionResult<object>> GetPrintBill(int orderId)
        {
            var query = await _service.QueryOrderWithDetails(orderId);
            var order = await query.FirstOrDefaultAsync();

            if (order == null)
                return NotFound("Không tìm thấy đơn hàng.");

            var result = new
            {
                orderCode = order.OrderCode,
                orderDate = order.OrderDate,
                customerName = order.Customer?.CustomerName,
                items = order.OrderDetails.Select(od => new
                {
                    productId = od.ProductId,
                    productName = od.Product?.ProductName,
                    quantity = od.Quantity,
                    unitPrice = od.UnitPrice
                }).ToList(),
                totalAmount = order.TotalAmount,
                taxAmount = order.TaxAmount,
                netAmount = order.NetAmount
            };

            return Ok(result);
        }
    }
}
