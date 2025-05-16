using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrderDetailsController : ControllerBase
    {
        private readonly IPurchaseOrderDetailService _service;
        public PurchaseOrderDetailsController(IPurchaseOrderDetailService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<PurchaseOrderDetailDto>>> GetAll() => Ok(await _service.GetAllPurchaseOrderDetailsAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseOrderDetailDto>> Get(int id)
        {
            try { return Ok(await _service.GetPurchaseOrderDetailByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseOrderDetailDto>> Create(CreatePurchaseOrderDetailDto dto)
        {
            var created = await _service.AddPurchaseOrderDetailAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.PurchaseOrderDetailId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdatePurchaseOrderDetailDto dto)
        {
            if (id != dto.PurchaseOrderDetailId) return BadRequest("ID mismatch");
            var result = await _service.UpdatePurchaseOrderDetailAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeletePurchaseOrderDetailAsync(id);
            return NoContent();
        }
    }
}
