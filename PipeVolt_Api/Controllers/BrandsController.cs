using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_BLL.Services;
using PipeVolt_DAL.DTOS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipeVolt_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
        }

        /// <summary>
        /// Lấy danh sách tất cả các thương hiệu
        /// </summary>
        /// <returns>Danh sách BrandDto</returns>
        [HttpGet]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetAllBrands()
        {
            try
            {
                var brands = await _brandService.GetAllBrandsAsync();
                return Ok(brands);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách thương hiệu: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin thương hiệu theo ID
        /// </summary>
        /// <param name="id">ID của thương hiệu</param>
        /// <returns>BrandDto của thương hiệu</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<BrandDto>> GetBrandById(int id)
        {
            try
            {
                var brand = await _brandService.GetBrandByIdAsync(id);
                if (brand == null)
                    return NotFound("Thương hiệu không tồn tại.");
                return Ok(brand);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy thương hiệu: {ex.Message}");
            }
        }

        /// <summary>
        /// Thêm thương hiệu mới
        /// </summary>
        /// <param name="dto">Thông tin thương hiệu mới</param>
        /// <returns>BrandDto của thương hiệu vừa tạo</returns>
        [HttpPost]
        public async Task<ActionResult<BrandDto>> CreateBrand([FromBody] CreateBrandDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var brandDto = new BrandDto { BrandName = dto.BrandName };
                var createdBrand = await _brandService.AddBrandAsync(brandDto);
                return CreatedAtAction(nameof(GetBrandById), new { id = createdBrand.BrandId }, createdBrand);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi thêm thương hiệu: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin thương hiệu
        /// </summary>
        /// <param name="id">ID của thương hiệu</param>
        /// <param name="dto">Thông tin cập nhật</param>
        /// <returns>BrandDto của thương hiệu đã cập nhật</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<BrandDto>> UpdateBrand(int id, [FromBody] UpdateBrandDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var brandDto = new BrandDto { BrandName = dto.BrandName };
                var updatedBrand = await _brandService.UpdateBrandAsync(id, brandDto);
                return Ok(updatedBrand);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật thương hiệu: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa thương hiệu theo ID
        /// </summary>
        /// <param name="id">ID của thương hiệu</param>
        /// <returns>Trạng thái xóa</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            try
            {
                var result = await _brandService.DeleteProductAsync(id); 
                if (!result)
                    return NotFound("Thương hiệu không tồn tại.");
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa thương hiệu: {ex.Message}");
            }
        }
    }
}