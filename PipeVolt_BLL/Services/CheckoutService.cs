using AutoMapper;
using Microsoft.EntityFrameworkCore; // Added for Include and FirstOrDefaultAsync
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.Common;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using static PipeVolt_DAL.Common.DataType;

namespace PipeVolt_BLL.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IGenericRepository<Cart> _cartRepo;
        private readonly IGenericRepository<CartItem> _cartItemRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<SalesOrder> _salesOrderRepo;
        private readonly IGenericRepository<OrderDetail> _orderDetailRepo;
        private readonly IGenericRepository<Invoice> _invoiceRepo;
        private readonly IGenericRepository<InvoiceDetail> _invoiceDetailRepo;
        private readonly IGenericRepository<Inventory> _inventoryRepo;
        private readonly IGenericRepository<PaymentTransaction> _paymentTransactionRepo;
        private readonly IGenericRepository<PaymentMethod> _paymentMethodRepo;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CheckoutService(
            IGenericRepository<Cart> cartRepo,
            IGenericRepository<CartItem> cartItemRepo,
            IGenericRepository<Product> productRepo,
            IGenericRepository<SalesOrder> salesOrderRepo,
            IGenericRepository<OrderDetail> orderDetailRepo,
            IGenericRepository<Invoice> invoiceRepo,
            IGenericRepository<InvoiceDetail> invoiceDetailRepo,
            IGenericRepository<Inventory> inventoryRepo,
            IGenericRepository<PaymentTransaction> paymentTransactionRepo,
            IGenericRepository<PaymentMethod> paymentMethodRepo,
            IMapper mapper,
            ILoggerService logger,
            IUnitOfWork unitOfWork)
        {
            _cartRepo = cartRepo ?? throw new ArgumentNullException(nameof(cartRepo));
            _cartItemRepo = cartItemRepo ?? throw new ArgumentNullException(nameof(cartItemRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _salesOrderRepo = salesOrderRepo ?? throw new ArgumentNullException(nameof(salesOrderRepo));
            _orderDetailRepo = orderDetailRepo ?? throw new ArgumentNullException(nameof(orderDetailRepo));
            _invoiceRepo = invoiceRepo ?? throw new ArgumentNullException(nameof(invoiceRepo));
            _invoiceDetailRepo = invoiceDetailRepo ?? throw new ArgumentNullException(nameof(invoiceDetailRepo));
            _inventoryRepo = inventoryRepo ?? throw new ArgumentNullException(nameof(inventoryRepo));
            _paymentTransactionRepo = paymentTransactionRepo ?? throw new ArgumentNullException(nameof(paymentTransactionRepo));
            _paymentMethodRepo = paymentMethodRepo ?? throw new ArgumentNullException(nameof(paymentMethodRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<int> CheckoutAsync(int customerId, int paymentMethodId)
        {
            _logger.LogInformation($"Starting checkout process for customer {customerId} with payment method {paymentMethodId}");

            using (var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Kiểm tra giỏ hàng
                    var cartQuery = await _cartRepo.QueryBy(c => c.CustomerId == customerId);
                    var cart = await cartQuery
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                        .Include(c => c.Customer)
                        .FirstOrDefaultAsync();

                    if (cart == null)
                    {
                        _logger.LogWarning($"Cart not found for customer {customerId}");
                        throw new KeyNotFoundException("Giỏ hàng không tồn tại.");
                    }

                    if (!cart.CartItems.Any())
                    {
                        _logger.LogWarning($"Cart is empty for customer {customerId}");
                        throw new InvalidOperationException("Giỏ hàng trống.");
                    }

                    // 2. Kiểm tra phương thức thanh toán
                    var paymentMethodQuery = await _paymentMethodRepo.QueryBy(pm => pm.PaymentMethodId == paymentMethodId);
                    var paymentMethod = await paymentMethodQuery.FirstOrDefaultAsync();
                    if (paymentMethod == null)
                    {
                        _logger.LogWarning($"Payment method {paymentMethodId} not found");
                        throw new InvalidOperationException("Phương thức thanh toán không tồn tại.");
                    }

                    // 3. Kiểm tra tồn kho
                    foreach (var item in cart.CartItems)
                    {
                        var inventoryQuery = await _inventoryRepo.QueryBy(i => i.ProductId == item.ProductId);
                        var inventory = await inventoryQuery.FirstOrDefaultAsync();
                        if (inventory == null || inventory.Quantity < item.Quantity)
                        {
                            _logger.LogWarning($"Insufficient stock for product {item.ProductId}");
                            throw new InvalidOperationException($"Sản phẩm {item.Product?.ProductName} không đủ tồn kho.");
                        }
                    }

                    // 4. Tạo SalesOrder
                    var salesOrder = new SalesOrder
                    {
                        OrderCode = GenerateOrderCode(),
                        CustomerId = customerId,
                        OrderDate = DateTime.Now,
                        TotalAmount = cart.CartItems.Sum(ci => ci.LineTotal),
                        DiscountAmount = 0, // Có thể thêm logic giảm giá
                        TaxAmount = cart.CartItems.Sum(ci => ci.LineTotal) * 0.1, // Giả sử thuế 10%
                        NetAmount = cart.CartItems.Sum(ci => ci.LineTotal) * 1.1, // Tổng sau thuế
                        Status = (int)SaleStatus.Completed,
                        PaymentMethodId = paymentMethodId
                    };

                    var createdOrder = await _salesOrderRepo.Create(salesOrder);

                    // 5. Tạo OrderDetail
                    var orderDetails = cart.CartItems.Select(ci => new OrderDetail
                    {
                        OrderId = createdOrder.OrderId,
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.UnitPrice,
                        Discount = 0, // Có thể thêm logic giảm giá
                        LineTotal = ci.Quantity * ci.UnitPrice
                    }).ToList();

                    await _orderDetailRepo.Create(orderDetails);

                    // 6. Tạo Invoice
                    var invoice = new Invoice
                    {
                        InvoiceNumber = GenerateInvoiceNumber(),
                        OrderId = createdOrder.OrderId,
                        CustomerId = customerId,
                        CustomerName = cart.Customer?.CustomerName ?? "Unknown",
                        CustomerAddress = cart.Customer?.Address,
                        CustomerPhone = cart.Customer?.Phone,
                        CustomerTaxCode = cart.Customer?.CustomerCode,
                        IssueDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(30),
                        SubTotal = salesOrder.TotalAmount ?? 0,
                        VatRate = 0.1,
                        VatAmount = salesOrder.TaxAmount ?? 0,
                        TotalAmount = salesOrder.NetAmount ?? 0,
                        Status = (int)DataType.InvoiceStatus.Draft,
                        PaymentStatus =(int)DataType.PaymentStatus.UnPaid
                    };

                    var createdInvoice = await _invoiceRepo.Create(invoice);

                    // 7. Tạo InvoiceDetail
                    var invoiceDetails = cart.CartItems.Select(ci => new InvoiceDetail
                    {
                        InvoiceId = createdInvoice.InvoiceId,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product?.ProductName ?? "Unknown",
                        ProductCode = ci.Product?.ProductCode ?? "Unknown",
                        Unit = ci.Product?.Unit,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.UnitPrice,
                        Discount = 0
                    }).ToList();

                    await _invoiceDetailRepo.Create(invoiceDetails);

                    // 8. Tạo PaymentTransaction
                    var paymentTransaction = new PaymentTransaction
                    {
                        OrderId = createdOrder.OrderId,
                        PaymentMethodId = paymentMethodId,
                        TransactionCode = GenerateTransactionCode(),
                        Amount = salesOrder.NetAmount ?? 0,
                        TransactionDate = DateTime.Now,
                        Status = (int)PaymentTransactionStatus.Suscess // Giả sử thanh toán thành công
                    };

                    await _paymentTransactionRepo.Create(paymentTransaction);

                    // 9. Cập nhật tồn kho
                    foreach (var item in cart.CartItems)
                    {
                        var inventoryQuery = await _inventoryRepo.QueryBy(i => i.ProductId == item.ProductId);
                        var inventory = await inventoryQuery.FirstOrDefaultAsync();
                        inventory.Quantity -= item.Quantity;
                        inventory.UpdatedAt = DateTime.Now;
                        await _inventoryRepo.Update(inventory);
                    }

                    // 10. Xóa CartItems
                    await _cartItemRepo.DeleteRange(ci => ci.CartId == cart.CartId);

                    // 11. Commit transaction
                    await transaction.CommitAsync();
                    _logger.LogInformation($"Checkout completed for customer {customerId}. Order ID: {createdOrder.OrderId}");

                    return createdOrder.OrderId;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Checkout failed for customer {customerId}", ex);
                    throw;
                }
            }
        }

        public async Task<int> CreateOrderAndCheckoutAsync(int customerId, int paymentMethodId, List<int> cartItemIds)
        {
            _logger.LogInformation($"Starting partial checkout for customer {customerId} with payment method {paymentMethodId} and cart items {string.Join(",", cartItemIds)}");

            using (var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Kiểm tra giỏ hàng và các CartItem được chọn
                    var cartQuery = await _cartRepo.QueryBy(c => c.CustomerId == customerId);
                    var cart = await cartQuery
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                        .Include(c => c.Customer)
                        .FirstOrDefaultAsync();

                    if (cart == null)
                    {
                        _logger.LogWarning($"Cart not found for customer {customerId}");
                        throw new KeyNotFoundException("Giỏ hàng không tồn tại.");
                    }

                    var selectedCartItems = cart.CartItems
                        .Where(ci => cartItemIds.Contains(ci.CartItemId))
                        .ToList();

                    if (!selectedCartItems.Any())
                    {
                        _logger.LogWarning($"No valid cart items selected for customer {customerId}");
                        throw new InvalidOperationException("Không có sản phẩm nào được chọn để thanh toán.");
                    }

                    // 2. Kiểm tra phương thức thanh toán
                    var paymentMethodQuery = await _paymentMethodRepo.QueryBy(pm => pm.PaymentMethodId == paymentMethodId);
                    var paymentMethod = await paymentMethodQuery.FirstOrDefaultAsync();
                    if (paymentMethod == null)
                    {
                        _logger.LogWarning($"Payment method {paymentMethodId} not found");
                        throw new InvalidOperationException("Phương thức thanh toán không tồn tại.");
                    }

                    // 3. Kiểm tra tồn kho
                    foreach (var item in selectedCartItems)
                    {
                        var inventoryQuery = await _inventoryRepo.QueryBy(i => i.ProductId == item.ProductId);
                        var inventory = await inventoryQuery.FirstOrDefaultAsync();
                        if (inventory == null || inventory.Quantity < item.Quantity)
                        {
                            _logger.LogWarning($"Insufficient stock for product {item.ProductId}");
                            throw new InvalidOperationException($"Sản phẩm {item.Product?.ProductName} không đủ tồn kho.");
                        }
                    }

                    // 4. Tạo SalesOrder
                    var salesOrder = new SalesOrder
                    {
                        OrderCode = GenerateOrderCode(),
                        CustomerId = customerId,
                        OrderDate = DateTime.Now,
                        TotalAmount = selectedCartItems.Sum(ci => ci.LineTotal),
                        DiscountAmount = 0,
                        TaxAmount = selectedCartItems.Sum(ci => ci.LineTotal) * 0.1,
                        NetAmount = selectedCartItems.Sum(ci => ci.LineTotal) * 1.1,
                        Status = (int)SaleStatus.Pending,
                        PaymentMethodId = paymentMethodId
                    };

                    var createdOrder = await _salesOrderRepo.Create(salesOrder);

                    // 5. Tạo OrderDetail
                    var orderDetails = selectedCartItems.Select(ci => new OrderDetail
                    {
                        OrderId = createdOrder.OrderId,
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.UnitPrice,
                        Discount = 0,
                        LineTotal = ci.Quantity * ci.UnitPrice
                    }).ToList();

                    await _orderDetailRepo.Create(orderDetails);

                    // 6. Tạo PaymentTransaction
                    var paymentTransaction = new PaymentTransaction
                    {
                        OrderId = createdOrder.OrderId,
                        PaymentMethodId = paymentMethodId,
                        TransactionCode = GenerateTransactionCode(),
                        Amount = salesOrder.NetAmount ?? 0,
                        TransactionDate = DateTime.Now,
                        Status = (int)SaleStatus.Pending
                    };

                    await _paymentTransactionRepo.Create(paymentTransaction);

                    // 7. Giả lập thanh toán
                    bool paymentSuccess = await ProcessPayment(paymentTransaction);

                    if (!paymentSuccess)
                    {
                        paymentTransaction.Status = (int)PaymentTransactionStatus.Failed;
                        salesOrder.Status =(int)SaleStatus.Pending;
                        await _paymentTransactionRepo.Update(paymentTransaction);
                        await _salesOrderRepo.Update(salesOrder);
                        await transaction.CommitAsync();
                        _logger.LogWarning($"Payment failed for order {createdOrder.OrderId}");
                        throw new InvalidOperationException("Thanh toán thất bại.");
                    }

                    // 8. Cập nhật trạng thái thanh toán thành công
                    paymentTransaction.Status = (int)PaymentTransactionStatus.Suscess;
                    salesOrder.Status = (int)SaleStatus.Paid;
                    await _paymentTransactionRepo.Update(paymentTransaction);
                    // 9. Tạo Invoice
                    var invoice = new Invoice
                    {
                        InvoiceNumber = GenerateInvoiceNumber(),
                        OrderId = createdOrder.OrderId,
                        CustomerId = customerId,
                        CustomerName = cart.Customer?.CustomerName ?? "Unknown",
                        CustomerAddress = cart.Customer?.Address,
                        CustomerPhone = cart.Customer?.Phone,
                        CustomerTaxCode = cart.Customer?.CustomerCode,
                        IssueDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(30),
                        SubTotal = salesOrder.TotalAmount ?? 0,
                        VatRate = 0.1,
                        VatAmount = salesOrder.TaxAmount ?? 0,
                        TotalAmount = salesOrder.NetAmount ?? 0,
                        Status =(int)DataType.InvoiceStatus.Paid,
                        PaymentStatus = (int)DataType.PaymentStatus.Paid
                    };

                    var createdInvoice = await _invoiceRepo.Create(invoice);

                    // 10. Tạo InvoiceDetail
                    var invoiceDetails = selectedCartItems.Select(ci => new InvoiceDetail
                    {
                        InvoiceId = createdInvoice.InvoiceId,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product?.ProductName ?? "Unknown",
                        ProductCode = ci.Product?.ProductCode ?? "Unknown",
                        Unit = ci.Product?.Unit,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.UnitPrice,
                        Discount = 0
                    }).ToList();

                    await _invoiceDetailRepo.Create(invoiceDetails);

                    // 11. Cập nhật tồn kho
                    foreach (var item in selectedCartItems)
                    {
                        var inventoryQuery = await _inventoryRepo.QueryBy(i => i.ProductId == item.ProductId);
                        var inventory = await inventoryQuery.FirstOrDefaultAsync();
                        inventory.Quantity -= item.Quantity;
                        inventory.UpdatedAt = DateTime.Now;
                        await _inventoryRepo.Update(inventory);
                    }

                    // 12. Xóa các CartItem được thanh toán
                    await _cartItemRepo.DeleteRange(ci => cartItemIds.Contains(ci.CartItemId));

                    // 13. Commit transaction
                    await transaction.CommitAsync();
                    _logger.LogInformation($"Partial checkout completed for customer {customerId}. Order ID: {createdOrder.OrderId}");

                    return createdOrder.OrderId;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Partial checkout failed for customer {customerId}", ex);
                    throw;
                }
            }
        }

        // Giả lập xử lý thanh toán
        private async Task<bool> ProcessPayment(PaymentTransaction transaction)
        {
            await Task.Delay(100); // Giả lập thời gian xử lý
            var random = new Random();
            return random.NextDouble() > 0.2; // 80% thành công
        }



        private string GenerateOrderCode()
        {
            return $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        private string GenerateTransactionCode()
        {
            return $"TXN-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}