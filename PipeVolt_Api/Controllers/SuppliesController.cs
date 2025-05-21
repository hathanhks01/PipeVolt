using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliesController : ControllerBase
    {
        private readonly ISupplyService _service;
        public SuppliesController(ISupplyService service) => _service = service;

        [HttpGet]
        [Route("GetList")]
        public async Task<ActionResult<List<SupplyDto>>> GetAll() => Ok(await _service.GetAllSuppliesAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<SupplyDto>> Get(int id)
        {
            try { return Ok(await _service.GetSupplyByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public async Task<ActionResult<SupplyDto>> Create(CreateSupplyDto dto)
        {
            var created = await _service.AddSupplyAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.SupplyId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSupplyDto dto)
        {
            if (id != dto.SupplyId) return BadRequest("ID mismatch");
            var result = await _service.UpdateSupplyAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteSupplyAsync(id);
            return NoContent();
        }
    }

}
