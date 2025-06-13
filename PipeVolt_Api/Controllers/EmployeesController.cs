using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _svc;
        public EmployeesController(IEmployeeService svc) => _svc = svc;

        [HttpGet]
        [Route("GetList")]
        public async Task<IActionResult> GetAll()
            => Ok(await _svc.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _svc.GetByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                new { id = created.EmployeeId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto)
        {
            if (id != dto.EmployeeId) return BadRequest("ID mismatch");
            try
            {
                var updated = await _svc.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try { await _svc.DeleteAsync(id); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }
        [HttpPost("generateAccount")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> GenerateAccount(int EmployeeId)
        {
            try
            {
                var result = await _svc.GenerateUserAccountForEmployee(EmployeeId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }

}
