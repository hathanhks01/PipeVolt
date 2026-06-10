// PipeVolt_BLL/Services/ProductToolService.cs
using Microsoft.EntityFrameworkCore;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.Models;
using System.Text.Json;

namespace PipeVolt_BLL.Services
{
    public class ProductToolService : IProductToolService
    {
        private readonly PipeVoltDbContext _context;
        private readonly ILoggerService _logger;

        public ProductToolService(PipeVoltDbContext context, ILoggerService logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> ExecuteToolAsync(string toolName, Dictionary<string, object>? parameters)
        {
            parameters ??= new Dictionary<string, object>();
            _logger.LogInformation($"[ProductToolService] Executing tool: {toolName}");

            return toolName switch
            {
                "search_products" => await SearchProductsAsync(
                    GetParam<string>(parameters, "keyword") ?? "",
                    GetParamNullable<int>(parameters, "categoryId")),

                "get_product_detail" => await GetProductDetailAsync(
                    GetParam<int>(parameters, "productId")),

                "check_inventory" => await CheckInventoryAsync(
                    GetParam<int>(parameters, "productId")),

                "get_categories" => await GetCategoriesAsync(),

                "get_products_by_price_range" => await GetProductsByPriceRangeAsync(
                    GetParam<double>(parameters, "minPrice"),
                    GetParam<double>(parameters, "maxPrice")),

                _ => JsonSerializer.Serialize(new { error = $"Tool '{toolName}' không tồn tại." })
            };
        }

        // ── search_products ────────────────────────────────────────────────────
        public async Task<string> SearchProductsAsync(string keyword, int? categoryId = null)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim();
                    query = query.Where(p =>
                        p.ProductName.Contains(kw) ||
                        (p.Description != null && p.Description.Contains(kw)) ||
                        (p.Category != null && p.Category.CategoryName.Contains(kw)) ||
                        (p.Brand != null && p.Brand.BrandName.Contains(kw)) ||
                        p.ProductCode.Contains(kw));
                }

                // Chỉ lọc theo category nếu được chỉ định rõ ràng
                if (categoryId.HasValue && categoryId.Value > 0)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                var products = await query.Take(8).ToListAsync();

                if (!products.Any())
                    return JsonSerializer.Serialize(new
                    {
                        found = false,
                        // ← Không gợi ý danh mục khác, chỉ báo không có
                        message = $"Cửa hàng không có sản phẩm '{keyword}'."
                    });

                var productIds = products.Select(p => p.ProductId).ToList();
                var stockSet = await _context.Inventories
                    .Where(i => productIds.Contains(i.ProductId))
                    .GroupBy(i => i.ProductId)
                    .Select(g => new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) })
                    .Where(x => x.Total > 0)
                    .Select(x => x.ProductId)
                    .ToHashSetAsync();

                var result = products.Select(p => new
                {
                    productId   = p.ProductId,
                    productName = p.ProductName,
                    // ← Sản phẩm không có danh mục vẫn hiển thị
                    category    = p.Category?.CategoryName ?? "Chưa phân loại",
                    brand       = p.Brand?.BrandName,
                    price       = FormatPrice(p.SellingPrice),
                    unit        = p.Unit,
                    summary     = TruncateDescription(p.Description),
                    inStock     = stockSet.Contains(p.ProductId)
                });

                return JsonSerializer.Serialize(new
                {
                    found    = true,
                    count    = products.Count,
                    products = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("SearchProductsAsync error", ex);
                return JsonSerializer.Serialize(new { error = "Lỗi khi tìm kiếm sản phẩm." });
            }
        }

        // ── get_product_detail ────────────────────────────────────────────────
        public async Task<string> GetProductDetailAsync(int productId)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                    return JsonSerializer.Serialize(new
                    {
                        found   = false,
                        message = $"Không tìm thấy sản phẩm ID {productId}."
                    });

                var totalStock = await _context.Inventories
                    .Where(i => i.ProductId == productId)
                    .SumAsync(i => i.Quantity);

                // Đầu ra công khai – không lộ giá nhập, số tồn kho thực tế
                return JsonSerializer.Serialize(new
                {
                    found       = true,
                    productId   = product.ProductId,
                    productName = product.ProductName,
                    category    = product.Category?.CategoryName ?? "Chưa phân loại",
                    brand       = product.Brand?.BrandName,
                    price       = FormatPrice(product.SellingPrice),
                    unit        = product.Unit,
                    description = product.Description,
                    imageUrl    = product.ImageUrl,
                    inStock     = totalStock > 0,
                    stockStatus = totalStock > 0 ? "Còn hàng" : "Hết hàng"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetProductDetailAsync error", ex);
                return JsonSerializer.Serialize(new { error = "Lỗi khi lấy thông tin sản phẩm." });
            }
        }

        // ── check_inventory ────────────────────────────────────────────────────
        public async Task<string> CheckInventoryAsync(int productId)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                    return JsonSerializer.Serialize(new
                    {
                        found   = false,
                        message = "Sản phẩm không tồn tại."
                    });

                var totalStock = await _context.Inventories
                    .Where(i => i.ProductId == productId)
                    .SumAsync(i => i.Quantity);

                // CHỈ trả về trạng thái tồn kho – không lộ số lượng thực và tên kho
                return JsonSerializer.Serialize(new
                {
                    found       = true,
                    productName = product.ProductName,
                    inStock     = totalStock > 0,
                    stockStatus = totalStock > 0 ? "Còn hàng" : "Hết hàng"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("CheckInventoryAsync error", ex);
                return JsonSerializer.Serialize(new { error = "Lỗi khi kiểm tra tồn kho." });
            }
        }

        // ── get_categories ─────────────────────────────────────────────────────
        public async Task<string> GetCategoriesAsync()
        {
            try
            {
                // Chỉ trả về danh mục có ít nhất 1 sản phẩm
                var categories = await _context.ProductCategories
                    .Where(c => _context.Products.Any(p => p.CategoryId == c.CategoryId))
                    .Select(c => new
                    {
                        categoryId   = c.CategoryId,
                        categoryName = c.CategoryName,
                        description  = c.Description
                    })
                    .ToListAsync();

                return JsonSerializer.Serialize(new
                {
                    count      = categories.Count,
                    categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetCategoriesAsync error", ex);
                return JsonSerializer.Serialize(new { error = "Lỗi khi lấy danh mục." });
            }
        }

        // ── get_products_by_price_range ────────────────────────────────────────
        public async Task<string> GetProductsByPriceRangeAsync(double minPrice, double maxPrice)
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Where(p => p.SellingPrice >= minPrice && p.SellingPrice <= maxPrice)
                    .Take(10)
                    .ToListAsync();

                if (!products.Any())
                    return JsonSerializer.Serialize(new
                    {
                        found   = false,
                        message = $"Không có sản phẩm trong khoảng giá {minPrice:N0}đ – {maxPrice:N0}đ."
                    });

                var productIds = products.Select(p => p.ProductId).ToList();
                var stockSet = await _context.Inventories
                    .Where(i => productIds.Contains(i.ProductId))
                    .GroupBy(i => i.ProductId)
                    .Select(g => new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) })
                    .Where(x => x.Total > 0)
                    .Select(x => x.ProductId)
                    .ToHashSetAsync();

                var result = products.Select(p => new
                {
                    productId   = p.ProductId,
                    productName = p.ProductName,
                    category    = p.Category?.CategoryName ?? "Chưa phân loại",
                    brand       = p.Brand?.BrandName,
                    price       = FormatPrice(p.SellingPrice),
                    unit        = p.Unit,
                    inStock     = stockSet.Contains(p.ProductId)
                });

                return JsonSerializer.Serialize(new
                {
                    found    = true,
                    count    = products.Count,
                    products = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetProductsByPriceRangeAsync error", ex);
                return JsonSerializer.Serialize(new { error = "Lỗi khi lọc theo giá." });
            }
        }

        // ── private helpers ────────────────────────────────────────────────────

        /// <summary>Định dạng giá bán – ẩn nếu null.</summary>
        private static string FormatPrice(double? price)
            => price.HasValue ? $"{price.Value:N0} đ" : "Liên hệ";

        /// <summary>Rút gọn mô tả xuống ≤150 ký tự để tránh lộ chi tiết nội bộ.</summary>
        private static string? TruncateDescription(string? desc, int maxLen = 150)
        {
            if (string.IsNullOrWhiteSpace(desc)) return null;
            desc = desc.Trim();
            return desc.Length <= maxLen ? desc : desc[..maxLen] + "…";
        }

        private static T GetParam<T>(Dictionary<string, object> p, string key)
        {
            if (!p.TryGetValue(key, out var val)) return default!;
            if (val is JsonElement je) return je.Deserialize<T>()!;
            return (T)Convert.ChangeType(val, typeof(T));
        }

        private static T? GetParamNullable<T>(Dictionary<string, object> p, string key) where T : struct
        {
            if (!p.TryGetValue(key, out var val) || val == null) return null;
            if (val is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Null) return null;
                // categoryId = 0 nghĩa là không lọc danh mục
                var v = je.Deserialize<T>();
                if (v is int i && i == 0) return null;
                return v;
            }
            return (T)Convert.ChangeType(val, typeof(T));
        }
    }
}