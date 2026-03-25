using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // ========== 1. REVENUE & PROFIT REPORT ==========
    public class RevenueProfitReportDto
    {
        public DateTime ReportDate { get; set; }
        public double TotalRevenue { get; set; }        // Tổng doanh thu
        public double TotalCost { get; set; }           // Tổng chi phí vốn
        public double TotalGrossProfit { get; set; }    // Lợi nhuận gộp
        public double TotalOperatingExpense { get; set; } // Chi phí hoạt động
        public double TotalNetProfit { get; set; }      // Lợi nhuận ròng
        public double ProfitMargin { get; set; }        // Tỷ suất lợi nhuận (%)
        public string Period { get; set; }              // Ngày/Tháng/Năm
    }

    public class RevenueDetailDto
    {
        public DateTime Date { get; set; }
        public double Revenue { get; set; }
        public double Cost { get; set; }
        public double GrossProfit { get; set; }
        public double NetProfit { get; set; }
        public string Period { get; set; }
    }

    public class RevenueComparisonDto
    {
        public string Period { get; set; }
        public double CurrentRevenue { get; set; }
        public double PreviousRevenue { get; set; }
        public double Difference { get; set; }
        public double PercentageChange { get; set; }
    }

    // ========== 2. SALES REPORT ==========
    public class SalesReportDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int TotalQuantitySold { get; set; }      // Tổng số lượng bán
        public double TotalRevenue { get; set; }        // Tổng doanh thu
        public double AverageUnitPrice { get; set; }    // Giá bán trung bình
        public int SalesCount { get; set; }             // Số lần bán
        public int Rank { get; set; }                   // Xếp hạng
    }

    public class SalesTrendDto
    {
        public DateTime Date { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public double Revenue { get; set; }
        public string Period { get; set; } // Ngày/Tháng/Năm
    }

    public class DailySalesChartDto
    {
        public string Date { get; set; }
        public double TotalSales { get; set; }
        public int OrderCount { get; set; }
    }

    // ========== 3. INVENTORY REPORT ==========
    public class InventoryReportDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int TotalQuantity { get; set; }          // Tổng tồn kho
        public int LowStockQuantity { get; set; }       // Hàng sắp hết
        public int OverStockQuantity { get; set; }      // Hàng tồn thừa
        public double TotalValue { get; set; }          // Giá trị tồn kho
        public double UnitCost { get; set; }            // Giá vốn
        public string Status { get; set; }              // Normal/LowStock/OverStock
    }

    public class WarehouseInventoryDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double Value { get; set; }
    }

    public class InventoryMovementDto
    {
        public DateTime Date { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string MovementType { get; set; }        // In/Out
        public string Reference { get; set; }           // SalesOrder/PurchaseOrder/etc
        public int? ReferenceId { get; set; }
    }

    // ========== 4. CUSTOMER REPORT ==========
    public class CustomerReportDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public int TotalOrders { get; set; }            // Tổng số đơn hàng
        public double TotalPurchaseAmount { get; set; } // Tổng tiền mua
        public double AverageOrderValue { get; set; }   // Giá trị trung bình/đơn
        public DateTime LastOrderDate { get; set; }     // Ngày mua cuối cùng
        public int DaysSinceLastPurchase { get; set; }  // Số ngày không mua
        public string CustomerSegment { get; set; }     // VIP/Regular/AtRisk
    }

    public class CustomerCohortDto
    {
        public string Cohort { get; set; }              // Period when customer acquired
        public int CustomerCount { get; set; }
        public double TotalRevenue { get; set; }
        public double AverageRevenuePerCustomer { get; set; }
    }

    public class NewCustomerDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public double TotalFirstPurchase { get; set; }
        public string Period { get; set; }
    }

    // ========== 5. PURCHASE & SUPPLIER REPORT ==========
    public class PurchaseReportDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }
        public string Contact { get; set; }
        public int TotalPurchaseOrders { get; set; }    // Tổng số PO
        public double TotalPurchaseAmount { get; set; } // Tổng tiền nhập
        public double AverageOrderValue { get; set; }   // Giá trị trung bình/đơn
        public int TotalProductCount { get; set; }      // Số loại sản phẩm
        public double AverageDeliveryDays { get; set; } // Thời gian giao hàng trung bình
        public int OnTimeDeliveryCount { get; set; }    // Số lần giao đúng hạn
        public double OnTimeRate { get; set; }          // Tỷ lệ giao đúng hạn (%)
    }

    public class SupplierProductDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double UnitCost { get; set; }            // Giá nhập
        public int TotalQuantityPurchased { get; set; } // Tổng số lượng nhập
        public double TotalPurchaseAmount { get; set; }
    }

    public class PurchaseTrendDto
    {
        public DateTime Date { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int PurchaseOrderCount { get; set; }
        public double Amount { get; set; }
        public string Period { get; set; }
    }

    // ========== 7. COST & PROFIT MARGIN REPORT ==========
    public class CostProfitMarginDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public double UnitCost { get; set; }            // Giá vốn
        public double AverageSalePrice { get; set; }    // Giá bán trung bình
        public double GrossProfitPerUnit { get; set; }  // Lợi nhuận gộp/đơn
        public double ProfitMarginPercent { get; set; } // Tỷ suất lợi nhuận (%)
        public int QuantitySold { get; set; }           // Số lượng bán
        public double TotalGrossProfit { get; set; }    // Tổng lợi nhuận gộp
        public string ProfitLevel { get; set; }         // High/Medium/Low
    }

    public class CategoryProfitAnalysisDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public double TotalRevenue { get; set; }
        public double TotalCost { get; set; }
        public double TotalGrossProfit { get; set; }
        public double ProfitMarginPercent { get; set; }
        public int ProductCount { get; set; }           // Số sản phẩm trong danh mục
        public int TotalQuantitySold { get; set; }
    }

    public class BrandProfitAnalysisDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public double TotalRevenue { get; set; }
        public double TotalCost { get; set; }
        public double TotalGrossProfit { get; set; }
        public double ProfitMarginPercent { get; set; }
        public int ProductCount { get; set; }
        public int TotalQuantitySold { get; set; }
    }

    // ========== REPORT REQUEST FILTERS ==========
    public class ReportFilterDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string PeriodType { get; set; }          // Daily/Monthly/Yearly
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public int? Top { get; set; }                   // Top N records
    }

    public class ReportResponseDto<T>
    {
        public DateTime GeneratedAt { get; set; }
        public string ReportType { get; set; }
        public string Period { get; set; }
        public int TotalRecords { get; set; }
        public List<T> Data { get; set; }
        public Dictionary<string, object> Summary { get; set; } // Tóm tắt (tổng, trung bình, etc)
    }
}
