using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipeVolt_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   // [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILoggerService _logger;

        public ReportsController(IReportService reportService, ILoggerService logger)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ========== 1. REVENUE & PROFIT REPORT ==========
        [HttpPost("revenue-profit")]
        public async Task<ActionResult<RevenueDetailDto>> GetRevenueProfit([FromBody] ReportFilterDto filter)
        {
            try
            {
                if (filter == null || filter.FromDate == default || filter.ToDate == default)
                    return BadRequest(new { error = "FromDate and ToDate are required" });

                var result = await _reportService.GetRevenueProfitAsync(filter.FromDate, filter.ToDate);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetRevenueProfit", ex);
                return BadRequest(new { error = "Failed to generate report", message = ex.Message });
            }
        }

        [HttpPost("revenue-trend")]
        public async Task<ActionResult<List<RevenueDetailDto>>> GetRevenueTrend([FromBody] ReportFilterDto filter)
        {
            try
            {
                if (filter == null || filter.FromDate == default || filter.ToDate == default)
                    return BadRequest(new { error = "FromDate and ToDate are required" });

                var periodType = filter.PeriodType ?? "Daily";
                var result = await _reportService.GetRevenueTrendAsync(filter.FromDate, filter.ToDate, periodType);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetRevenueTrend", ex);
                return BadRequest(new { error = "Failed to generate report", message = ex.Message });
            }
        }

        // ========== 2. SALES REPORT ==========
        [HttpPost("top-sellers")]
        public async Task<ActionResult<List<SalesReportDto>>> GetTopSellers([FromBody] ReportFilterDto filter)
        {
            try
            {
                if (filter == null || filter.FromDate == default || filter.ToDate == default)
                    return BadRequest(new { error = "FromDate and ToDate are required" });

                int top = filter.Top ?? 10;
                var result = await _reportService.GetTopSellersAsync(filter.FromDate, filter.ToDate, top);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetTopSellers", ex);
                return BadRequest(new { error = "Failed to generate report", message = ex.Message });
            }
        }

        // ========== 3. INVENTORY REPORT ==========
        [HttpGet("inventory-status")]
        public async Task<ActionResult<List<InventoryReportDto>>> GetInventoryStatus()
        {
            try
            {
                var result = await _reportService.GetInventoryStatusAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetInventoryStatus", ex);
                return BadRequest(new { error = "Failed to generate report", message = ex.Message });
            }
        }

        // ========== 4. CUSTOMER REPORT ==========
        [HttpPost("customer-analysis")]
        public async Task<ActionResult<List<CustomerReportDto>>> GetCustomerAnalysis([FromBody] ReportFilterDto filter)
        {
            try
            {
                if (filter == null || filter.FromDate == default || filter.ToDate == default)
                    return BadRequest(new { error = "FromDate and ToDate are required" });

                var result = await _reportService.GetCustomerAnalysisAsync(filter.FromDate, filter.ToDate);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetCustomerAnalysis", ex);
                return BadRequest(new { error = "Failed to generate report", message = ex.Message });
            }
        }

        // ========== 5. PURCHASE & SUPPLIER REPORT ==========
        [HttpPost("supplier-analysis")]
        public async Task<ActionResult<List<PurchaseReportDto>>> GetSupplierAnalysis([FromBody] ReportFilterDto filter)
        {
            try
            {
                if (filter == null || filter.FromDate == default || filter.ToDate == default)
                    return BadRequest(new { error = "FromDate and ToDate are required" });

                var result = await _reportService.GetSupplierAnalysisAsync(filter.FromDate, filter.ToDate);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetSupplierAnalysis", ex);
                return BadRequest(new { error = "Failed to generate report", message = ex.Message });
            }
        }

        // ========== 7. COST & PROFIT MARGIN REPORT ==========
        [HttpPost("profit-margin-analysis")]
        public async Task<ActionResult<List<CostProfitMarginDto>>> GetProfitMarginAnalysis([FromBody] ReportFilterDto filter)
        {
            try
            {
                if (filter == null || filter.FromDate == default || filter.ToDate == default)
                    return BadRequest(new { error = "FromDate and ToDate are required" });

                var result = await _reportService.GetProfitMarginAnalysisAsync(filter.FromDate, filter.ToDate);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetProfitMarginAnalysis", ex);
                return BadRequest(new { error = "Failed to generate report", message = ex.Message });
            }
        }

        [HttpPost("category-profit-analysis")]
        public async Task<ActionResult<List<CategoryProfitAnalysisDto>>> GetCategoryProfitAnalysis([FromBody] ReportFilterDto filter)
        {
            try
            {
                if (filter == null || filter.FromDate == default || filter.ToDate == default)
                    return BadRequest(new { error = "FromDate and ToDate are required" });

                var result = await _reportService.GetCategoryProfitAnalysisAsync(filter.FromDate, filter.ToDate);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetCategoryProfitAnalysis", ex);
                return BadRequest(new { error = "Failed to generate report", message = ex.Message });
            }
        }
    }
}
