using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.Services;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _service;
        public WarehousesController(IWarehouseService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<WarehouseDto>>> GetAll() => Ok(await _service.GetAllWarehousesAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<WarehouseDto>> Get(int id)
        {
            try { return Ok(await _service.GetWarehouseByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public async Task<ActionResult<WarehouseDto>> Create([FromBody] CreateWarehouseDto dto)
        {
            var created = await _service.AddWarehouseAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.WarehouseId }, created);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateWarehouseDto dto)
        {
            if (id != dto.WarehouseId) return BadRequest("ID mismatch");
            var result = await _service.UpdateWarehouseAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteWarehouseAsync(id);
            return NoContent();
        }
    }

}
