using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipeVolt_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        /// <summary>
        /// Lấy danh sách tất cả khách hàng
        /// </summary>
        /// <returns>Danh sách CustomerDto</returns>
        [HttpGet]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách khách hàng: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin khách hàng theo ID
        /// </summary>
        /// <param name="id">ID của khách hàng</param>
        /// <returns>CustomerDto của khách hàng</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy khách hàng: {ex.Message}");
            }
        }

        /// <summary>
        /// Thêm khách hàng mới
        /// </summary>
        /// <param name="dto">Thông tin khách hàng mới</param>
        /// <returns>CustomerDto của khách hàng vừa tạo</returns>
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdCustomer = await _customerService.AddCustomerAsync(dto);
                return CreatedAtAction(nameof(GetCustomerById), new { id = createdCustomer.CustomerId }, createdCustomer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi thêm khách hàng: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="id">ID của khách hàng</param>
        /// <param name="dto">Thông tin cập nhật</param>
        /// <returns>CustomerDto của khách hàng đã cập nhật</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerDto>> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dto.CustomerId)
            {
                return BadRequest("Customer ID mismatch.");
            }

            try
            {
                var updatedCustomer = await _customerService.UpdateCustomerAsync(id, dto);
                return Ok(updatedCustomer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật khách hàng: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa khách hàng theo ID
        /// </summary>
        /// <param name="id">ID của khách hàng</param>
        /// <returns>Trạng thái xóa</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa khách hàng: {ex.Message}");
            }
        }
        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerByUserId(int userId)
        {
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);
            if (customer == null)
                return NotFound("Customer not found for this userId.");
            return Ok(customer);
        }
    }
}