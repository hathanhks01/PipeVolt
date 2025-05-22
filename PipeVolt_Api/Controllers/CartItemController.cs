using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_BLL.Services;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartItemController : ControllerBase
    {
        private readonly ICartItemService _cartItemService;
        private readonly ILoggerService _logger;

        public CartItemController(ICartItemService cartItemService, ILoggerService logger)
        {
            _cartItemService = cartItemService;
            _logger = logger;
        }

        [HttpGet("cart/{cartId}")]
        public async Task<IActionResult> GetCartItems(int cartId)
        {
            try
            {
                _logger.LogInformation($"[GET] Getting items for cart ID {cartId}");
                var result = await _cartItemService.GetCartItemsByCartIdAsync(cartId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting cart items for cart ID {cartId}", ex);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("cart/{cartId}/add")]
        public async Task<IActionResult> AddCartItem(int cartId, [FromBody] AddCartItemDto dto)
        {
            try
            {
                _logger.LogInformation($"[POST] Adding item to cart ID {cartId}");
                var result = await _cartItemService.AddCartItemAsync(cartId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding item to cart ID {cartId}", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto dto)
        {
            try
            {
                _logger.LogInformation($"[PUT] Updating cart item ID {dto.CartItemId}");
                var success = await _cartItemService.UpdateCartItemAsync(dto);
                if (!success) return NotFound("Cart item not found");

                return Ok("Cart item updated");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating cart item ID {dto.CartItemId}", ex);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> DeleteCartItem(int cartItemId)
        {
            try
            {
                _logger.LogInformation($"[DELETE] Deleting cart item ID {cartItemId}");
                var success = await _cartItemService.DeleteCartItemAsync(cartItemId);
                if (!success) return NotFound("Cart item not found");

                return Ok("Cart item deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting cart item ID {cartItemId}", ex);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
