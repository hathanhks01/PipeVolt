using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _service;
        public PurchaseOrdersController(IPurchaseOrderService service) => _service = service;

        [HttpGet]
        [Route("GetList")]
        public async Task<ActionResult<List<PurchaseOrderDto>>> GetAll() => Ok(await _service.GetAllPurchaseOrdersAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseOrderDto>> Get(int id)
        {
            try { return Ok(await _service.GetPurchaseOrderByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost("add-PurchaseOrders")]
        public async Task<ActionResult<PurchaseOrderDto>> Create(CreatePurchaseOrderDto dto)
        {
            try
            {
                var created = await _service.AddPurchaseOrderAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = created.PurchaseOrderId }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to create purchase order", message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdatePurchaseOrderDto dto)
        {
            if (id != dto.PurchaseOrderId) return BadRequest("ID mismatch");
            var result = await _service.UpdatePurchaseOrderAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeletePurchaseOrderAsync(id);
            return NoContent();
        }
    }
}
