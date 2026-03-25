using PipeVolt_DAL.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.IServices
{
    public interface IReportService
    {
        // 1. Revenue & Profit Report
        Task<RevenueDetailDto> GetRevenueProfitAsync(DateTime fromDate, DateTime toDate);
        Task<List<RevenueDetailDto>> GetRevenueTrendAsync(DateTime fromDate, DateTime toDate, string periodType = "Daily");

        // 2. Sales Report
        Task<List<SalesReportDto>> GetTopSellersAsync(DateTime fromDate, DateTime toDate, int top = 10);

        // 3. Inventory Report
        Task<List<InventoryReportDto>> GetInventoryStatusAsync();

        // 4. Customer Report
        Task<List<CustomerReportDto>> GetCustomerAnalysisAsync(DateTime fromDate, DateTime toDate);

        // 5. Purchase & Supplier Report
        Task<List<PurchaseReportDto>> GetSupplierAnalysisAsync(DateTime fromDate, DateTime toDate);

        // 7. Cost & Profit Margin Report
        Task<List<CostProfitMarginDto>> GetProfitMarginAnalysisAsync(DateTime fromDate, DateTime toDate);
        Task<List<CategoryProfitAnalysisDto>> GetCategoryProfitAnalysisAsync(DateTime fromDate, DateTime toDate);
    }
}
