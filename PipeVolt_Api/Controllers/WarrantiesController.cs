using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.Services;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarrantiesController : ControllerBase
    {
        private readonly IWarrantyService _service;
        public WarrantiesController(IWarrantyService service) => _service = service;

        [HttpGet]
        [Route("GetList")]
        public async Task<ActionResult<List<WarrantyDto>>> GetAll() => Ok(await _service.GetAllWarrantiesAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<WarrantyDto>> Get(int id)
        {
            try { return Ok(await _service.GetWarrantyByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public async Task<ActionResult<WarrantyDto>> Create([FromBody] CreateWarrantyDto dto)
        {
            var created = await _service.AddWarrantyAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.WarrantyId }, created);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateWarrantyDto dto)
        {
            if (id != dto.WarrantyId) return BadRequest("ID mismatch");
            var result = await _service.UpdateWarrantyAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteWarrantyAsync(id);
            return NoContent();
        }
    }
}
