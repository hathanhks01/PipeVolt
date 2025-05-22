using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.Services;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        /// <summary>
        /// Lấy giỏ hàng của khách hàng theo customerId
        /// </summary>
        /// <param name="customerId">ID của khách hàng</param>
        /// <returns>CartDto chứa thông tin giỏ hàng</returns>
        [HttpGet("{customerId}")]
        public async Task<ActionResult<CartDto>> GetCart(int customerId)
        {
            try
            {
                var cart = await _cartService.GetCartAsync(customerId);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo giỏ hàng mới cho khách hàng
        /// </summary>
        /// <param name="dto">Dữ liệu để tạo giỏ hàng</param>
        /// <returns>CartDto của giỏ hàng vừa tạo</returns>
        [HttpPost]
        public async Task<ActionResult<CartDto>> CreateCart(CreateCartDto dto)
        {
            try
            {
                var createdCart = await _cartService.AddCartAsync(dto);
                return CreatedAtAction(nameof(GetCart), new { customerId = createdCart.CustomerId }, createdCart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin giỏ hàng
        /// </summary>
        /// <param name="id">ID của giỏ hàng</param>
        /// <param name="dto">Dữ liệu cập nhật</param>
        /// <returns>CartDto của giỏ hàng đã cập nhật</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<CartDto>> UpdateCart(int id, UpdateCartDto dto)
        {
            try
            {
                if (id != dto.CartId)
                {
                    return BadRequest("Cart ID mismatch.");
                }
                var updatedCart = await _cartService.UpdateCartAsync(id, dto);
                return Ok(updatedCart);
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa giỏ hàng
        /// </summary>
        /// <param name="id">ID của giỏ hàng</param>
        /// <returns>No content nếu xóa thành công</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            try
            {
                var result = await _cartService.DeleteCartAsync(id);
                if (!result)
                {
                    return NotFound("Cart not found.");
                }
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        /// <param name="customerId">ID của khách hàng</param>
        /// <param name="itemDto">Dữ liệu sản phẩm cần thêm</param>
        /// <returns>CartDto của giỏ hàng sau khi thêm</returns>
        [HttpPost("{customerId}/items")]
        public async Task<ActionResult<CartDto>> AddItemToCart(int customerId, AddCartItemDto itemDto)
        {
            try
            {
                var updatedCart = await _cartService.AddItemToCartAsync(customerId, itemDto);
                return Ok(updatedCart);
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        /// <param name="customerId">ID của khách hàng</param>
        /// <param name="itemDto">Dữ liệu cập nhật sản phẩm</param>
        /// <returns>CartDto của giỏ hàng sau khi cập nhật</returns>
        [HttpPut("{customerId}/items")]
        public async Task<ActionResult<CartDto>> UpdateCartItem(int customerId, UpdateCartItemDto itemDto)
        {
            try
            {
                var updatedCart = await _cartService.UpdateCartItemAsync(customerId, itemDto);
                return Ok(updatedCart);
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        /// <param name="customerId">ID của khách hàng</param>
        /// <param name="cartItemId">ID của mục giỏ hàng</param>
        /// <returns>CartDto của giỏ hàng sau khi xóa</returns>
        [HttpDelete("{customerId}/items/{cartItemId}")]
        public async Task<ActionResult<CartDto>> RemoveCartItem(int customerId, int cartItemId)
        {
            try
            {
                var updatedCart = await _cartService.RemoveCartItemAsync(customerId, cartItemId);
                return Ok(updatedCart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Thanh toán giỏ hàng và tạo đơn hàng
        /// </summary>
        /// <param name="customerId">ID của khách hàng</param>
        /// <returns>ID của SalesOrder vừa tạo</returns>
        [HttpPost("{customerId}/checkout")]
        public async Task<ActionResult<int>> Checkout(int customerId)
        {
            try
            {
                var orderId = await _cartService.CheckoutAsync(customerId);
                return Ok(orderId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
