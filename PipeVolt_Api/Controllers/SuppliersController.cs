using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _service;
        public SuppliersController(ISupplierService service) => _service = service;

        [HttpGet]
        [Route("GetList")]
        public async Task<ActionResult<List<SupplierDto>>> GetAll() => Ok(await _service.GetAllSuppliersAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierDto>> Get(int id)
        {
            try { return Ok(await _service.GetSupplierByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public async Task<ActionResult<SupplierDto>> Create(CreateSupplierDto dto)
        {
            var created = await _service.AddSupplierAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.SupplierId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSupplierDto dto)
        {
            if (id != dto.SupplierId) return BadRequest("ID mismatch");
            var result = await _service.UpdateSupplierAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteSupplierAsync(id);
            return NoContent();
        }
    }

}
