using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;

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
    }

}
