using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipeVolt_DAL.Models;
using PipeVolt_DAL.Common;
using System;
using System.Threading.Tasks;
using System.Linq;
using PipeVolt_BLL.IServices;

namespace PipeVolt_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SepayController : ControllerBase
    {
        private readonly PipeVoltDbContext _context;
        private readonly ICheckoutService _checkoutService;

        public SepayController(PipeVoltDbContext context, ICheckoutService checkoutService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _checkoutService = checkoutService;
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
                    var matchWithDash = System.Text.RegularExpressions.Regex.Match(content, @"ORD-\d{14}-[a-zA-Z0-9]{8}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    var matchWithoutDash = System.Text.RegularExpressions.Regex.Match(content, @"ORD\d{14}[a-zA-Z0-9]{8}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    if (matchWithDash.Success)
                    {
                        orderCode = matchWithDash.Value;
                    }
                    else if (matchWithoutDash.Success)
                    {
                        var rawCode = matchWithoutDash.Value;
                        orderCode = $"ORD-{rawCode.Substring(3, 14)}-{rawCode.Substring(17, 8).ToLower()}";
                    }
                }

                if (!string.IsNullOrWhiteSpace(orderCode))
                {
                    var cleanCodeMatch = System.Text.RegularExpressions.Regex.Match(orderCode, @"^ORD\d{14}[a-zA-Z0-9]{8}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (cleanCodeMatch.Success)
                    {
                        orderCode = $"ORD-{orderCode.Substring(3, 14)}-{orderCode.Substring(17, 8).ToLower()}";
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
                if (salesOrder.Status == (int)DataType.SaleStatus.Completed || salesOrder.Status == (int)DataType.SaleStatus.shipping)
                {
                    return Ok(new { Message = "Payment already processed and completed" });
                }

                // 4. Cập nhật trạng thái đơn hàng sang shipping
                salesOrder.Status = (int)DataType.SaleStatus.shipping;

                // Cập nhật thông tin tài chính cho SalesOrder từ số tiền thực tế chuyển khoản
                // TransferAmount = NetAmount (Tổng tiền sau thuế VAT 10%)
                double soNetAmount = (double)request.TransferAmount;
                double soTotalAmount = Math.Round(soNetAmount / 1.1, 2); // Tiền hàng chưa thuế
                double soTaxAmount = Math.Round(soNetAmount - soTotalAmount, 2);  // Tiền thuế VAT

                salesOrder.NetAmount = soNetAmount;
                salesOrder.TotalAmount = soTotalAmount;
                salesOrder.TaxAmount = soTaxAmount;

                // Trừ inventory FIFO theo các OrderDetail của đơn này
                var orderDetails = await _context.OrderDetails
                    .Where(od => od.OrderId == salesOrder.OrderId)
                    .ToListAsync();

                foreach (var detail in orderDetails)
                {
                    var inventoryList = await _context.Inventories
                        .Where(i => i.ProductId == detail.ProductId)
                        .OrderBy(i => i.UpdatedAt)
                        .ToListAsync();

                    int remaining = detail.Quantity ?? 0;
                    foreach (var inv in inventoryList)
                    {
                        if (remaining <= 0) break;
                        int deduct = Math.Min(inv.Quantity, remaining);
                        inv.Quantity -= deduct;
                        inv.UpdatedAt = DateTime.Now;
                        remaining -= deduct;
                    }

                    if (remaining > 0)
                    {
                        // Log cảnh báo — không throw để tránh webhook retry loop
                        Console.WriteLine($"[Sepay webhook] WARNING: insufficient stock for product {detail.ProductId}");
                    }
                }

                // Xóa các CartItem đã thanh toán
                var cartItemProductIds = orderDetails.Select(od => od.ProductId).ToList();
                if (salesOrder.CustomerId.HasValue)
                {
                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.CustomerId == salesOrder.CustomerId.Value);

                    if (cart != null)
                    {
                        var cartItemsToRemove = cart.CartItems
                            .Where(ci => cartItemProductIds.Contains(ci.ProductId))
                            .ToList();
                        _context.CartItems.RemoveRange(cartItemsToRemove);
                    }
                }

                // 5. Cập nhật hoặc tạo mới hóa đơn sang Paid
                var invoice = await _context.Invoices
                    .Include(i => i.InvoiceDetails)
                    .FirstOrDefaultAsync(i => i.OrderId == salesOrder.OrderId);

                double totalAmt = (double)request.TransferAmount;
                double vatRate = 0.1;
                double subTotal = Math.Round(totalAmt / 1.1, 2);
                double vatAmount = Math.Round(totalAmt - subTotal, 2);

                if (invoice != null)
                {
                    invoice.Status = (int)DataType.InvoiceStatus.Paid;
                    invoice.PaymentStatus = (int)DataType.PaymentStatus.Paid;
                    invoice.SubTotal = subTotal;
                    invoice.VatRate = vatRate;
                    invoice.VatAmount = vatAmount;
                    invoice.TotalAmount = totalAmt;
                    invoice.UpdatedAt = DateTime.Now;
                }
                else
                {
                    var customer = salesOrder.CustomerId.HasValue 
                        ? await _context.Customers.FindAsync(salesOrder.CustomerId.Value) 
                        : null;

                    invoice = new Invoice
                    {
                        InvoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                        OrderId = salesOrder.OrderId,
                        CustomerId = salesOrder.CustomerId,
                        CustomerName = customer?.CustomerName ?? "Unknown",
                        CustomerAddress = customer?.Address,
                        CustomerPhone = customer?.Phone,
                        CustomerTaxCode = customer?.TaxCode,
                        IssueDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(3),
                        SubTotal = subTotal,
                        VatRate = vatRate,
                        VatAmount = vatAmount,
                        TotalAmount = totalAmt,
                        Status = (int)DataType.InvoiceStatus.Paid,
                        PaymentStatus = (int)DataType.PaymentStatus.Paid,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    foreach (var detail in orderDetails)
                    {
                        var product = await _context.Products.FindAsync(detail.ProductId);
                        var invoiceDetail = new InvoiceDetail
                        {
                            ProductId = detail.ProductId,
                            ProductName = product?.ProductName ?? "Unknown",
                            ProductCode = product?.ProductCode ?? "Unknown",
                            Unit = product?.Unit,
                            Quantity = detail.Quantity ?? 0,
                            UnitPrice = detail.UnitPrice ?? 0,
                            Discount = detail.Discount ?? 0,
                            LineTotal = (detail.Quantity ?? 0) * (detail.UnitPrice ?? 0) - (detail.Discount ?? 0)
                        };
                        invoice.InvoiceDetails.Add(invoiceDetail);
                    }

                    _context.Invoices.Add(invoice);
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
