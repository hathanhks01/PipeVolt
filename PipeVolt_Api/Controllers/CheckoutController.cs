using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using System;
using System.Threading.Tasks;

namespace PipeVolt_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;

        public CheckoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService));
        }

        /// <summary>
        /// Thanh toán giỏ hàng và tạo đơn hàng
        /// </summary>
        /// <param name="customerId">ID của khách hàng</param>
        /// <param name="paymentMethodId">ID của phương thức thanh toán</param>
        /// <returns>ID của SalesOrder vừa tạo</returns>
        [HttpPost("{customerId}")]
        public async Task<ActionResult<int>> Checkout(int customerId, [FromBody] int paymentMethodId)
        {
            try
            {
                var orderId = await _checkoutService.CheckoutAsync(customerId, paymentMethodId);
                return Ok(orderId);
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
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }
        /// <summary>
        /// Thanh toán các sản phẩm được chọn từ giỏ hàng
        /// </summary>
        /// <param name="customerId">ID của khách hàng</param>
        /// <param name="request">Phương thức thanh toán và danh sách ID của CartItem</param>
        /// <returns>ID của SalesOrder vừa tạo</returns>
        [HttpPost("{customerId}/partial")]
        public async Task<ActionResult<int>> CheckoutPartial(int customerId, [FromBody] CheckoutPartialRequest request)
        {
            try
            {
                if (request == null || request.CartItemIds == null || !request.CartItemIds.Any())
                {
                    return BadRequest("Danh sách sản phẩm được chọn không hợp lệ.");
                }

                var orderId = await _checkoutService.CreateOrderAndCheckoutAsync(customerId, request.PaymentMethodId, request.CartItemIds);
                return Ok(orderId);
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
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }
    }
    public class CheckoutFullRequest
        {
            public int PaymentMethodId { get; set; }
        }

        public class CheckoutPartialRequest
        {
            public int PaymentMethodId { get; set; }
            public List<int> CartItemIds { get; set; }
        }
}