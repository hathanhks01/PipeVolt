using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_BLL.Services;
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
        /// <summary>
        /// Lấy danh sách sản phẩm tồn kho theo mã kho hàng
        /// </summary>
        /// <param name="warehouseCode">Mã kho hàng (WarehouseCode)</param>
        /// <returns>Danh sách sản phẩm tồn kho</returns>
        [HttpGet("warehouse/code/{warehouseCode}")]
        public async Task<ActionResult<List<InventoryProductDto>>> GetInventoriesByWarehouseCode(string warehouseCode)
        {
            try
            {
                var inventories = await _service.GetInventoriesByWarehouseCodeAsync(warehouseCode);
                return Ok(inventories);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpPost("ReceiveFromPurchaseOrder")]
        public async Task<IActionResult> ReceiveFromPurchaseOrder([FromBody] ReceivePurchaseOrderDto dto)
        {
            await _service.ReceiveFromPurchaseOrderAsync(dto.WarehouseCode, dto.PurchaseOrderId);
            return Ok("Nhập kho thành công từ đơn hàng mua.");
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
