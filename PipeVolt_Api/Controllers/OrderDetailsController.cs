using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.Services;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderDetailsController : ControllerBase
    {
        private readonly IOrderDetailService _service;
        public OrderDetailsController(IOrderDetailService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<OrderDetailDto>>> GetAll() => Ok(await _service.GetAllOrderDetailsAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailDto>> Get(int id)
        {
            try { return Ok(await _service.GetOrderDetailByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public async Task<ActionResult<OrderDetailDto>> Create(CreateOrderDetailDto dto)
        {
            var created = await _service.AddOrderDetailAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.OrderDetailId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateOrderDetailDto dto)
        {
            if (id != dto.OrderDetailId) return BadRequest("ID mismatch");
            var result = await _service.UpdateOrderDetailAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteOrderDetailAsync(id);
            return NoContent();
        }
    }

}
