using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _service;
        public InventoriesController(IInventoryService service) => _service = service;

        [HttpGet]
        [Route("GetList")]
        public async Task<ActionResult<List<InventoryDto>>> GetAll() => Ok(await _service.GetAllInventoriesAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryDto>> Get(int id)
        {
            try { return Ok(await _service.GetInventoryByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public async Task<ActionResult<InventoryDto>> Create(CreateInventoryDto dto)
        {
            var created = await _service.AddInventoryAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.InventoryId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateInventoryDto dto)
        {
            if (id != dto.InventoryId) return BadRequest("ID mismatch");
            var result = await _service.UpdateInventoryAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteInventoryAsync(id);
            return NoContent();
        }
    }
}
