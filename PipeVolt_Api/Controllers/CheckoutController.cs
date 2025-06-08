using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
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
        /// <summary>
        /// Thanh toán tại quầy (POS) không cần giỏ hàng
        /// </summary>
        /// <param name="request">Danh sách sản phẩm, phương thức thanh toán, thông tin khách hàng (tùy chọn), ID nhân viên thu ngân, và phần trăm giảm giá</param>
        /// <returns>ID của SalesOrder vừa tạo</returns>
        [HttpPost("pos")]
        public async Task<ActionResult<int>> PosCheckout([FromBody] PosCheckoutRequest request)
        {
            try
            {
                if (request == null || request.Items == null || !request.Items.Any())
                {
                    return BadRequest("Danh sách sản phẩm không hợp lệ.");
                }

                var orderId = await _checkoutService.PosCheckoutAsync(
                    request.Items,
                    request.PaymentMethodId,
                    request.CustomerInfo,
                    request.CashierId,
                    request.DiscountPercent);

                return Ok(orderId);
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

    // DTO for POS checkout request
    public class PosCheckoutRequest
    {
        public List<PosItem> Items { get; set; }
        public int PaymentMethodId { get; set; }
        public PosCustomerInfo? CustomerInfo { get; set; }
        public int? CashierId { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
    }
}

