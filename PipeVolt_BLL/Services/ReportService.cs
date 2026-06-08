using AutoMapper;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PipeVolt_DAL.Common;

namespace PipeVolt_BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IGenericRepository<SalesOrder> _salesOrderRepo;
        private readonly IGenericRepository<OrderDetail> _orderDetailRepo;
        private readonly IGenericRepository<PurchaseOrder> _purchaseOrderRepo;
        private readonly IGenericRepository<PurchaseOrderDetail> _purchaseOrderDetailRepo;
        private readonly IGenericRepository<Inventory> _inventoryRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<Customer> _customerRepo;
        private readonly IGenericRepository<Supplier> _supplierRepo;
        private readonly IGenericRepository<Invoice> _invoiceRepo;
        private readonly IGenericRepository<ProductCategory> _categoryRepo;
        private readonly IGenericRepository<Brand> _brandRepo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public ReportService(
            IGenericRepository<SalesOrder> salesOrderRepo,
            IGenericRepository<OrderDetail> orderDetailRepo,
            IGenericRepository<PurchaseOrder> purchaseOrderRepo,
            IGenericRepository<PurchaseOrderDetail> purchaseOrderDetailRepo,
            IGenericRepository<Inventory> inventoryRepo,
            IGenericRepository<Product> productRepo,
            IGenericRepository<Customer> customerRepo,
            IGenericRepository<Supplier> supplierRepo,
            IGenericRepository<Invoice> invoiceRepo,
            IGenericRepository<ProductCategory> categoryRepo,
            IGenericRepository<Brand> brandRepo,
            ILoggerService logger,
            IMapper mapper)
        {
            _salesOrderRepo = salesOrderRepo ?? throw new ArgumentNullException(nameof(salesOrderRepo));
            _orderDetailRepo = orderDetailRepo ?? throw new ArgumentNullException(nameof(orderDetailRepo));
            _purchaseOrderRepo = purchaseOrderRepo ?? throw new ArgumentNullException(nameof(purchaseOrderRepo));
            _purchaseOrderDetailRepo = purchaseOrderDetailRepo ?? throw new ArgumentNullException(nameof(purchaseOrderDetailRepo));
            _inventoryRepo = inventoryRepo ?? throw new ArgumentNullException(nameof(inventoryRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _customerRepo = customerRepo ?? throw new ArgumentNullException(nameof(customerRepo));
            _supplierRepo = supplierRepo ?? throw new ArgumentNullException(nameof(supplierRepo));
            _invoiceRepo = invoiceRepo ?? throw new ArgumentNullException(nameof(invoiceRepo));
            _categoryRepo = categoryRepo ?? throw new ArgumentNullException(nameof(categoryRepo));
            _brandRepo = brandRepo ?? throw new ArgumentNullException(nameof(brandRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // ========== 1. REVENUE & PROFIT REPORT ==========
        public async Task<RevenueDetailDto> GetRevenueProfitAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.LogInformation($"GetRevenueProfitAsync: {fromDate} to {toDate}");

                var salesOrders = await _salesOrderRepo.FindBy(
                    so => so.OrderDate >= fromDate && so.OrderDate <= toDate && (int?)so.Status == (int)DataType.SaleStatus.Completed
                );

                var orderDetails = await _orderDetailRepo.FindBy(
                    od => salesOrders.Select(so => so.OrderId).Contains(od.OrderId)
                );

                var purchaseOrderDetails = await _purchaseOrderDetailRepo.GetAll();
                var purchaseOrders = await _purchaseOrderRepo.GetAll();

                double totalRevenue = orderDetails.Sum(od => (od.Quantity ?? 0) * (od.UnitPrice ?? 0) - (od.Discount ?? 0));
                double totalCost = orderDetails.Sum(od => 
                {
                    var unitCost = GetLatestProductUnitCost(od.ProductId, purchaseOrderDetails, purchaseOrders);
                    return (od.Quantity ?? 0) * unitCost;
                });
                double grossProfit = totalRevenue - totalCost;
                double operatingExpense = 0;
                double netProfit = grossProfit - operatingExpense;
                double profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;

                return new RevenueDetailDto
                {
                    Date = DateTime.Now,
                    Revenue = totalRevenue,
                    Cost = totalCost,
                    GrossProfit = grossProfit,
                    NetProfit = netProfit,
                    Period = $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetRevenueProfitAsync", ex);
                throw;
            }
        }

        public async Task<List<RevenueDetailDto>> GetRevenueTrendAsync(DateTime fromDate, DateTime toDate, string periodType = "Daily")
        {
            try
            {
                _logger.LogInformation($"GetRevenueTrendAsync: {fromDate} to {toDate}, Period: {periodType}");

                var salesOrders = await _salesOrderRepo.FindBy(
                    so => so.OrderDate >= fromDate && so.OrderDate <= toDate && (int?)so.Status == 3                    
                );

                var orderDetails = await _orderDetailRepo.FindBy(
                    od => salesOrders.Select(so => so.OrderId).Contains(od.OrderId)
                );

                var result = new List<RevenueDetailDto>();

                var purchaseOrderDetails = await _purchaseOrderDetailRepo.GetAll();
                var purchaseOrders = await _purchaseOrderRepo.GetAll();

                if (periodType == "Daily")
                {
                    for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                    {
                        var dayDetails = orderDetails.Where(od => 
                            salesOrders.First(so => so.OrderId == od.OrderId).OrderDate?.Date == date.Date
                        ).ToList();

                        double revenue = dayDetails.Sum(od => (od.Quantity ?? 0) * (od.UnitPrice ?? 0) - (od.Discount ?? 0));
                        double cost = dayDetails.Sum(od => 
                        {
                            var unitCost = GetLatestProductUnitCost(od.ProductId, purchaseOrderDetails, purchaseOrders);
                            return (od.Quantity ?? 0) * unitCost;
                        });

                        result.Add(new RevenueDetailDto
                        {
                            Date = date,
                            Revenue = revenue,
                            Cost = cost,
                            GrossProfit = revenue - cost,
                            NetProfit = revenue - cost,
                            Period = date.ToString("dd/MM/yyyy")
                        });
                    }
                }
                else if (periodType == "Monthly")
                {
                    var months = new Dictionary<string, List<OrderDetail>>();
                    foreach (var detail in orderDetails)
                    {
                        var date = salesOrders.First(so => so.OrderId == detail.OrderId).OrderDate ?? DateTime.Now;
                        var monthKey = date.ToString("MM/yyyy");
                        if (!months.ContainsKey(monthKey))
                            months[monthKey] = new List<OrderDetail>();
                        months[monthKey].Add(detail);
                    }

                    foreach (var month in months)
                    {
                        double revenue = month.Value.Sum(od => (od.Quantity ?? 0) * (od.UnitPrice ?? 0) - (od.Discount ?? 0));
                        double cost = month.Value.Sum(od => 
                        {
                            var unitCost = GetLatestProductUnitCost(od.ProductId, purchaseOrderDetails, purchaseOrders);
                            return (od.Quantity ?? 0) * unitCost;
                        });

                        result.Add(new RevenueDetailDto
                        {
                            Date = DateTime.ParseExact(month.Key, "MM/yyyy", null),
                            Revenue = revenue,
                            Cost = cost,
                            GrossProfit = revenue - cost,
                            NetProfit = revenue - cost,
                            Period = month.Key
                        });
                    }
                }

                return result.OrderBy(r => r.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetRevenueTrendAsync", ex);
                throw;
            }
        }

        // ========== 2. SALES REPORT ==========
        public async Task<List<SalesReportDto>> GetTopSellersAsync(DateTime fromDate, DateTime toDate, int top = 10)
        {
            try
            {
                _logger.LogInformation($"GetTopSellersAsync: {fromDate} to {toDate}, Top: {top}");

                var salesOrders = await _salesOrderRepo.FindBy(
                    so => so.OrderDate >= fromDate && so.OrderDate <= toDate && (int?)so.Status == 3
                );

                var orderDetails = await _orderDetailRepo.FindBy(
                    od => salesOrders.Select(so => so.OrderId).Contains(od.OrderId)
                );

                var products = await _productRepo.GetAll();
                var categories = await _categoryRepo.GetAll();
                var brands = await _brandRepo.GetAll();

                var salesSummary = orderDetails
                    .GroupBy(od => od.ProductId)
                    .Select((g, idx) => new SalesReportDto
                    {
                        ProductId = g.Key,
                        ProductName = products.FirstOrDefault(p => p.ProductId == g.Key)?.ProductName ?? "Unknown",
                        ProductCode = products.FirstOrDefault(p => p.ProductId == g.Key)?.ProductCode ?? "",
                        CategoryId = products.FirstOrDefault(p => p.ProductId == g.Key)?.CategoryId ?? 0,
                        CategoryName = categories.FirstOrDefault(c => c.CategoryId == products.FirstOrDefault(p => p.ProductId == g.Key)?.CategoryId)?.CategoryName ?? "",
                        BrandId = products.FirstOrDefault(p => p.ProductId == g.Key)?.BrandId ?? 0,
                        BrandName = brands.FirstOrDefault(b => b.BrandId == products.FirstOrDefault(p => p.ProductId == g.Key)?.BrandId)?.BrandName ?? "",
                        TotalQuantitySold = g.Sum(od => od.Quantity ?? 0),
                        TotalRevenue = g.Sum(od => (od.Quantity ?? 0) * (od.UnitPrice ?? 0) - (od.Discount ?? 0)),
                        AverageUnitPrice = g.Count() > 0 ? g.Average(od => od.UnitPrice ?? 0) : 0,
                        SalesCount = g.Count(),
                        Rank = idx + 1
                    })
                    .OrderByDescending(s => s.TotalRevenue)
                    .Take(top)
                    .ToList();

                // Cập nhật rank
                for (int i = 0; i < salesSummary.Count; i++)
                    salesSummary[i].Rank = i + 1;

                return salesSummary;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetTopSellersAsync", ex);
                throw;
            }
        }

        // ========== 3. INVENTORY REPORT ==========
        public async Task<List<InventoryReportDto>> GetInventoryStatusAsync()
        {
            try
            {
                _logger.LogInformation("GetInventoryStatusAsync");

                var inventories = (await _inventoryRepo.GetAll()).ToList();
                var products = (await _productRepo.GetAll()).ToList();
                var categories = (await _categoryRepo.GetAll()).ToList();

                var purchaseOrderDetails = await _purchaseOrderDetailRepo.GetAll();
                var purchaseOrders = await _purchaseOrderRepo.GetAll();

                var result = inventories
                    .GroupBy(inv => inv.ProductId)
                    .Select(g => {
                        var product = products.FirstOrDefault(p => p.ProductId == g.Key);
                        var unitCost = GetLatestProductUnitCost(g.Key, purchaseOrderDetails, purchaseOrders);
                        int quantity = g.Sum(inv => inv.Quantity);

                        return new InventoryReportDto
                        {
                            ProductId = g.Key,
                            ProductName = product?.ProductName ?? "Unknown",
                            ProductCode = product?.ProductCode ?? "",
                            CategoryId = product?.CategoryId ?? 0,
                            CategoryName = categories.FirstOrDefault(c => c.CategoryId == product?.CategoryId)?.CategoryName ?? "",
                            TotalQuantity = quantity,
                            LowStockQuantity = 10,
                            OverStockQuantity = 500,
                            TotalValue = quantity * unitCost,
                            UnitCost = unitCost,
                            Status = DetermineInventoryStatus(quantity, product)
                        };
                    })
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetInventoryStatusAsync", ex);
                throw;
            }
        }

        // ========== 4. CUSTOMER REPORT ==========
        public async Task<List<CustomerReportDto>> GetCustomerAnalysisAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.LogInformation($"GetCustomerAnalysisAsync: {fromDate} to {toDate}");

                var customers = await _customerRepo.GetAll();
                var salesOrders = await _salesOrderRepo.FindBy(
                    so => so.OrderDate >= fromDate && so.OrderDate <= toDate && (int?)so.Status == 3
                );

                var result = customers
                    .Select(c => new CustomerReportDto
                    {
                        CustomerId = c.CustomerId,
                        CustomerName = c.CustomerName ?? "Unknown",
                        CustomerCode = c.CustomerCode ?? "",
                        Address = c.Address ?? "",
                        TotalOrders = salesOrders.Count(so => so.CustomerId == c.CustomerId),
                        TotalPurchaseAmount = salesOrders
                            .Where(so => so.CustomerId == c.CustomerId)
                            .Sum(so => so.TotalAmount ?? 0),
                        AverageOrderValue = salesOrders.Where(so => so.CustomerId == c.CustomerId).Any()
                            ? salesOrders.Where(so => so.CustomerId == c.CustomerId).Average(so => so.TotalAmount ?? 0)
                            : 0,
                        LastOrderDate = salesOrders
                            .Where(so => so.CustomerId == c.CustomerId)
                            .OrderByDescending(so => so.OrderDate)
                            .FirstOrDefault()?.OrderDate ?? DateTime.MinValue,
                        DaysSinceLastPurchase = salesOrders.Where(so => so.CustomerId == c.CustomerId).Any()
                            ? (int)(DateTime.Now - (salesOrders.Where(so => so.CustomerId == c.CustomerId).OrderByDescending(so => so.OrderDate).First().OrderDate ?? DateTime.Now)).TotalDays
                            : 999,
                        CustomerSegment = DetermineCustomerSegment(
                            salesOrders.Where(so => so.CustomerId == c.CustomerId).Sum(so => so.TotalAmount ?? 0),
                            salesOrders.Count(so => so.CustomerId == c.CustomerId)
                        )
                    })
                    .Where(cr => cr.TotalOrders > 0)
                    .OrderByDescending(cr => cr.TotalPurchaseAmount)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetCustomerAnalysisAsync", ex);
                throw;
            }
        }

        // ========== 5. PURCHASE & SUPPLIER REPORT ==========
        public async Task<List<PurchaseReportDto>> GetSupplierAnalysisAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.LogInformation($"GetSupplierAnalysisAsync: {fromDate} to {toDate}");

                var suppliers = await _supplierRepo.GetAll();
                var purchaseOrders = await _purchaseOrderRepo.FindBy(
                    po => po.OrderDate >= fromDate && po.OrderDate <= toDate
                );

                var result = suppliers
                    .Select(s => new PurchaseReportDto
                    {
                        SupplierId = s.SupplierId,
                        SupplierName = s.SupplierName ?? "Unknown",
                        Contact = s.ContactPerson ?? "",
                        TotalPurchaseOrders = purchaseOrders.Count(po => po.SupplierId == s.SupplierId),
                        TotalPurchaseAmount = purchaseOrders
                            .Where(po => po.SupplierId == s.SupplierId)
                            .Sum(po => po.TotalAmount ?? 0),
                        AverageOrderValue = purchaseOrders.Where(po => po.SupplierId == s.SupplierId).Any()
                            ? purchaseOrders.Where(po => po.SupplierId == s.SupplierId).Average(po => po.TotalAmount ?? 0)
                            : 0,
                        TotalProductCount = purchaseOrders
                            .Where(po => po.SupplierId == s.SupplierId)
                            .SelectMany(po => _purchaseOrderDetailRepo.FindBy(pod => pod.PurchaseOrderId == po.PurchaseOrderId).Result)
                            .DistinctBy(pod => pod.ProductId)
                            .Count(),
                        AverageDeliveryDays = 0, // Cần thêm delivery_date vào model
                        OnTimeDeliveryCount = 0,   // Cần thêm logic
                        OnTimeRate = 0
                    })
                    .Where(pr => pr.TotalPurchaseOrders > 0)
                    .OrderByDescending(pr => pr.TotalPurchaseAmount)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetSupplierAnalysisAsync", ex);
                throw;
            }
        }

        // ========== 7. COST & PROFIT MARGIN REPORT ==========
        public async Task<List<CostProfitMarginDto>> GetProfitMarginAnalysisAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.LogInformation($"GetProfitMarginAnalysisAsync: {fromDate} to {toDate}");

                var salesOrders = await _salesOrderRepo.FindBy(
                    so => so.OrderDate >= fromDate && so.OrderDate <= toDate && (int?)so.Status == 3
                );

                var orderDetails = await _orderDetailRepo.FindBy(
                    od => salesOrders.Select(so => so.OrderId).Contains(od.OrderId)
                );

                var products = await _productRepo.GetAll();

                var purchaseOrderDetails = await _purchaseOrderDetailRepo.GetAll();
                var purchaseOrders = await _purchaseOrderRepo.GetAll();

                var result = orderDetails
                    .GroupBy(od => od.ProductId)
                    .Select(g => {
                        var unitCost = GetLatestProductUnitCost(g.Key, purchaseOrderDetails, purchaseOrders);
                        double avgSalePrice = g.Count() > 0 ? g.Average(od => od.UnitPrice ?? 0) : 0;
                        double grossProfitPerUnit = avgSalePrice - unitCost;
                        double marginPercent = CalculateMarginPercent(unitCost, avgSalePrice);
                        double totalGrossProfit = g.Sum(od => ((od.UnitPrice ?? 0) - unitCost) * (od.Quantity ?? 0));

                        return new CostProfitMarginDto
                        {
                            ProductId = g.Key,
                            ProductName = products.FirstOrDefault(p => p.ProductId == g.Key)?.ProductName ?? "Unknown",
                            ProductCode = products.FirstOrDefault(p => p.ProductId == g.Key)?.ProductCode ?? "",
                            UnitCost = unitCost,
                            AverageSalePrice = avgSalePrice,
                            GrossProfitPerUnit = grossProfitPerUnit,
                            ProfitMarginPercent = marginPercent,
                            QuantitySold = g.Sum(od => od.Quantity ?? 0),
                            TotalGrossProfit = totalGrossProfit,
                            ProfitLevel = DetermineProfitLevel(marginPercent)
                        };
                    })
                    .OrderByDescending(p => p.TotalGrossProfit)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetProfitMarginAnalysisAsync", ex);
                throw;
            }
        }

        public async Task<List<CategoryProfitAnalysisDto>> GetCategoryProfitAnalysisAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.LogInformation($"GetCategoryProfitAnalysisAsync: {fromDate} to {toDate}");

                var salesOrders = await _salesOrderRepo.FindBy(
                    so => so.OrderDate >= fromDate && so.OrderDate <= toDate && (int?)so.Status == 3
                );

                var orderDetails = await _orderDetailRepo.FindBy(
                    od => salesOrders.Select(so => so.OrderId).Contains(od.OrderId)
                );

                var products = await _productRepo.GetAll();
                var categories = await _categoryRepo.GetAll();

                var purchaseOrderDetails = await _purchaseOrderDetailRepo.GetAll();
                var purchaseOrders = await _purchaseOrderRepo.GetAll();

                var result = categories
                    .Select(cat => {
                        var catOrderDetails = orderDetails
                            .Where(od => products.FirstOrDefault(p => p.ProductId == od.ProductId)?.CategoryId == cat.CategoryId)
                            .ToList();

                        double totalRevenue = catOrderDetails.Sum(od => (od.Quantity ?? 0) * (od.UnitPrice ?? 0) - (od.Discount ?? 0));
                        double totalCost = catOrderDetails.Sum(od =>
                        {
                            var unitCost = GetLatestProductUnitCost(od.ProductId, purchaseOrderDetails, purchaseOrders);
                            return (od.Quantity ?? 0) * unitCost;
                        });

                        return new CategoryProfitAnalysisDto
                        {
                            CategoryId = cat.CategoryId,
                            CategoryName = cat.CategoryName ?? "Unknown",
                            TotalRevenue = totalRevenue,
                            TotalCost = totalCost,
                            TotalGrossProfit = totalRevenue - totalCost,
                            ProfitMarginPercent = totalRevenue > 0 ? ((totalRevenue - totalCost) / totalRevenue) * 100 : 0,
                            ProductCount = products.Count(p => p.CategoryId == cat.CategoryId),
                            TotalQuantitySold = catOrderDetails.Sum(od => od.Quantity ?? 0)
                        };
                    })
                    .Where(c => c.TotalRevenue > 0)
                    .OrderByDescending(c => c.TotalGrossProfit)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetCategoryProfitAnalysisAsync", ex);
                throw;
            }
        }

        // ========== HELPERS ==========
        /// <summary>
        /// Lấy unit cost từ đơn hàng nhập gần nhất của sản phẩm
        /// Nếu chưa có đơn hàng nhập, trả về 0
        /// </summary>
        private double GetLatestProductUnitCost(int productId, IEnumerable<PurchaseOrderDetail> purchaseOrderDetails, IEnumerable<PurchaseOrder> purchaseOrders)
        {
            var latestPOD = purchaseOrderDetails
                .Where(pod => pod.ProductId == productId)
                .Join(purchaseOrders,
                    pod => pod.PurchaseOrderId,
                    po => po.PurchaseOrderId,
                    (pod, po) => new { Pod = pod, Po = po })
                .OrderByDescending(x => x.Po.OrderDate)
                .FirstOrDefault();

            return latestPOD?.Pod.UnitCost ?? 0;
        }

        private string DetermineInventoryStatus(int quantity, Product product)
        {
            if (product == null) return "Unknown";
            if (quantity <= 10) return "LowStock";
            if (quantity >= 500) return "OverStock";
            return "Normal";
        }

        private string DetermineCustomerSegment(double totalPurchase, int orderCount)
        {
            if (totalPurchase > 50000000 || orderCount > 20) return "VIP";
            if (totalPurchase > 10000000 || orderCount > 5) return "Regular";
            return "AtRisk";
        }

        private double CalculateMarginPercent(double cost, double price)
        {
            if (price == 0) return 0;
            return ((price - cost) / price) * 100;
        }

        private string DetermineProfitLevel(double marginPercent)
        {
            if (marginPercent >= 30) return "High";
            if (marginPercent >= 15) return "Medium";
            return "Low";
        }
    }
}
