using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipeVolt_DAL.Models;
using PipeVolt_DAL.Common;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace PipeVolt_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SepayController : ControllerBase
    {
        private readonly PipeVoltDbContext _context;

        public SepayController(PipeVoltDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpPost("payment/access")]
        public async Task<IActionResult> Access([FromBody] SePayWebhookPayload request)
        {
            try
            {
                // 1. Kiểm tra dữ liệu đầu vào
                if (request == null)
                    return BadRequest(new { message = "Request payload is required" });

                // SePay webhook chỉ gửi thông tin nạp tiền vào (transfer type là "in")
                if (request.TransferType != "in")
                    return BadRequest(new { message = "Only 'in' transfer type is accepted" });

                // Validate thời gian giao dịch
                if (string.IsNullOrWhiteSpace(request.TransactionDate))
                    return BadRequest(new { message = "Transaction date is required" });

                if (!DateTime.TryParse(request.TransactionDate, out var transactionDate))
                    return BadRequest(new { message = "Invalid transaction date format. Expected: yyyy-MM-dd HH:mm:ss" });

                // 2. Tìm mã đơn hàng từ Code hoặc Content
                string? orderCode = request.Code;
                if (string.IsNullOrWhiteSpace(orderCode) && !string.IsNullOrWhiteSpace(request.Content))
                {
                    var content = request.Content;
                    var index = content.IndexOf("ORD-", StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        var potentialCode = content.Substring(index).Trim();
                        var spaceIndex = potentialCode.IndexOf(' ');
                        if (spaceIndex > 0)
                        {
                            orderCode = potentialCode.Substring(0, spaceIndex);
                        }
                        else
                        {
                            orderCode = potentialCode;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(orderCode))
                {
                    return BadRequest(new { message = "Cannot identify order code from transaction details" });
                }

                // 3. Truy vấn đơn hàng từ database
                var salesOrder = await _context.SalesOrders
                    .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

                if (salesOrder == null)
                {
                    return NotFound(new { message = $"Order not found for code: {orderCode}" });
                }

                // Nếu đơn hàng đã hoàn thành, trả về thành công để tránh SePay gửi lại
                if (salesOrder.Status == (int)DataType.SaleStatus.Completed)
                {
                    return Ok(new { Message = "Payment already processed and completed" });
                }

                // 4. Cập nhật trạng thái đơn hàng sang shipping
                salesOrder.Status = (int)DataType.SaleStatus.shipping;

                // 5. Cập nhật hóa đơn sang Paid
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.OrderId == salesOrder.OrderId);
                if (invoice != null)
                {
                    invoice.Status = (int)DataType.InvoiceStatus.Paid;
                    invoice.PaymentStatus = (int)DataType.PaymentStatus.Paid;
                }

                // 6. Ghi nhận giao dịch vào bảng PAYMENT_TRANSACTION
                string transactionCode = !string.IsNullOrWhiteSpace(request.ReferenceCode) 
                    ? request.ReferenceCode 
                    : $"SEPAY-{request.Id}";

                var existingTxn = await _context.PaymentTransactions
                    .FirstOrDefaultAsync(t => t.TransactionCode == transactionCode);

                if (existingTxn == null)
                {
                    var paymentTransaction = new PaymentTransaction
                    {
                        OrderId = salesOrder.OrderId,
                        PaymentMethodId = (int)salesOrder.PaymentMethodId , 
                        TransactionCode = transactionCode,
                        Amount = (double)request.TransferAmount,
                        TransactionDate = transactionDate,
                        Status = (int)DataType.PaymentTransactionStatus.Success,
                        GatewayResponse = $"SePay webhook id: {request.Id}, Gateway: {request.Gateway}, Account: {request.AccountNumber}, Accumulated: {request.Accumulated}, Content: {request.Content}"
                    };
                    _context.PaymentTransactions.Add(paymentTransaction);
                }

                // 7. Lưu các thay đổi vào Cơ sở dữ liệu
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Payment is working and recorded successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error processing payment", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Payload nhận được từ webhook của SePay gửi sang
    /// </summary>
    public class SePayWebhookPayload
    {
        public int Id { get; set; }
        public string Gateway { get; set; } = string.Empty;
        public string TransactionDate { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? SubAccount { get; set; }
        public string? Code { get; set; }
        public string Content { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal TransferAmount { get; set; }
        public decimal Accumulated { get; set; }
        public string? ReferenceCode { get; set; }
    }
}
