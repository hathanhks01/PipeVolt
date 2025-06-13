using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PipeVolt_Api.Common;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _repo;
        private readonly IGenericRepository<ProductCategory> _categoryRepo;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public ProductService(
            IGenericRepository<Product> repo,
            IGenericRepository<ProductCategory> categoryRepo,
            IMapper mapper,
            ILoggerService logger)
        {
            _repo = repo;
            _categoryRepo = categoryRepo ?? throw new ArgumentNullException(nameof(categoryRepo));
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            try
            {
                var sql = @"
            SELECT 
                p.product_id,
                p.product_code,
                p.product_name,
                p.category_id,
                p.brand_id,
                p.selling_price,
                p.unit,
                p.description,
                p.image_url,
                ISNULL(SUM(i.quantity), 0) AS quantity
            FROM PRODUCT p
            LEFT JOIN INVENTORY i ON p.product_id = i.product_id
            WHERE p.product_id = @p0
            GROUP BY 
                p.product_id,
                p.product_code,
                p.product_name,
                p.category_id,
                p.brand_id,
                p.selling_price,
                p.unit,
                p.description,
                p.image_url";

                var productSearch = await _repo.SqlQuery<Product>(sql,productId);
                var product = productSearch.FirstOrDefault();

                if (product == null)
                    throw new KeyNotFoundException("Product not found.");

                return _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetProductByIdAsync", ex);
                throw;
            }
        }

        public async Task<ProductDto> GetProductByIdAndRelatedAsync(int productId)
        {
            try
            {
                var products = await _repo.QueryBy(p => p.ProductId == productId);
                var product = await Task.Run(() => products.FirstOrDefault());
                if (product == null)
                    throw new KeyNotFoundException("Product not found.");

                // Lấy các sản phẩm liên quan: cùng CategoryId, khác ProductId hiện tại, lấy tối đa 5 sản phẩm
                List<Product> relatedProducts = new List<Product>();
                if (product.CategoryId.HasValue)
                {
                    var relatedQuery = await _repo.QueryBy(p =>
                        p.CategoryId == product.CategoryId &&
                        p.ProductId != product.ProductId);
                    relatedProducts = relatedQuery.Take(5).ToList();
                }

                var productDto = _mapper.Map<ProductDto>(product);

                // Nếu ProductDto chưa có property RelatedProducts, bạn cần thêm vào class ProductDto:
                // public List<ProductDto> RelatedProducts { get; set; }
                // Nếu đã có, gán như sau:
                if (productDto != null)
                    productDto.RelatedProducts = _mapper.Map<List<ProductDto>>(relatedProducts);

                return productDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetProductByIdAsync", ex);
                throw;
            }
        }
        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
           
                try
                {
                    var sql = @"
            SELECT 
                p.product_id,
                p.product_code,
                p.product_name,
                p.category_id,
                p.brand_id,
                p.selling_price,
                p.unit,
                p.description,
                p.image_url,
                ISNULL(SUM(i.quantity), 0) AS quantity
            FROM PRODUCT p
            LEFT JOIN INVENTORY i ON p.product_id = i.product_id
            GROUP BY 
                p.product_id,
                p.product_code,
                p.product_name,
                p.category_id,
                p.brand_id,
                p.selling_price,
                p.unit,
                p.description,
                p.image_url";

                    var product = await _repo.SqlQuery<Product>(sql);
                return _mapper.Map<IEnumerable<ProductDto>>(product);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in GetAllProductsAsync", ex);
                    throw;
                }
        }
        public async Task<IEnumerable<ProductDto>> GetPopularProductsAsync()
        {
            try
            {
                string sql = "SELECT TOP 5 * FROM PRODUCT ORDER BY product_name\r\n"; 
                var products = await _repo.SqlQuery<Product>(sql);
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetPopularProductsAsync", ex);
                throw;
            }
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            try
            {
                var entity = _mapper.Map<Product>(dto);

                if (dto.ImageFile != null)
                {
                    try
                    {
                        string imagePath = CommonFunctions.UploadFile(dto.ImageFile, "images/products");
                        entity.ImageUrl = imagePath;
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError("Image upload failed", ex);
                        throw new InvalidOperationException("Failed to upload image: " + ex.Message);
                    }
                }

                var created = await _repo.Create(entity);
                return _mapper.Map<ProductDto>(created);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in CreateProductAsync", ex);
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto dto)
        {
            try
            {
                var products = await _repo.QueryBy(p => p.ProductId == productId);
                var entity = await Task.Run(() => products.FirstOrDefault());
                if (entity == null)
                    throw new KeyNotFoundException("Product not found.");

                _mapper.Map(dto, entity);

                if (dto.ImageFile != null)
                {
                    try
                    {
                        string imagePath = CommonFunctions.UploadFile(dto.ImageFile, "images/products");
                        entity.ImageUrl = imagePath;
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError("Image upload failed", ex);
                        throw new InvalidOperationException("Failed to upload image: " + ex.Message);
                    }
                }

                await _repo.Update(entity);
                return _mapper.Map<ProductDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in UpdateProductAsync", ex);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                var products = await _repo.QueryBy(p => p.ProductId == productId);
                var entity = await Task.Run(() => products.FirstOrDefault());
                if (entity == null)
                    throw new KeyNotFoundException("Product not found.");

                // Xóa ảnh vật lý nếu tồn tại
                if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
                {
                    try
                    {
                        string fullImagePath = CommonFunctions.PhysicalPath(entity.ImageUrl);
                        if (File.Exists(fullImagePath))
                        {
                            File.Delete(fullImagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error deleting product image file", ex);
                        // Có thể không throw để vẫn cho phép xóa Product nếu chỉ xóa ảnh lỗi
                    }
                }

                await _repo.Delete(entity);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in DeleteProductAsync", ex);
                throw;
            }
        }
        // Phương thức mới: Lọc sản phẩm theo CategoryId
        public async Task<List<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            if (categoryId <= 0)
            {
                _logger.LogWarning("CategoryId không hợp lệ.");
                throw new ArgumentException("CategoryId phải lớn hơn 0.");
            }

            _logger.LogInformation($"Đang lấy danh sách sản phẩm thuộc danh mục {categoryId}");

            try
            {
                // Kiểm tra danh mục tồn tại
                var categoryQuery = await _categoryRepo.QueryBy(c => c.CategoryId == categoryId);
                var category = await categoryQuery.FirstOrDefaultAsync();
                if (category == null)
                {
                    _logger.LogWarning($"Không tìm thấy danh mục với ID {categoryId}");
                    throw new KeyNotFoundException($"Danh mục với ID {categoryId} không tồn tại.");
                }

                // Lấy sản phẩm theo CategoryId
                var productQuery = await _repo.QueryBy(p => p.CategoryId == categoryId);
                var products = await productQuery
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .ToListAsync();

                if (!products.Any())
                {
                    _logger.LogWarning($"Không tìm thấy sản phẩm nào thuộc danh mục {categoryId}");
                    throw new KeyNotFoundException($"Không có sản phẩm nào thuộc danh mục {categoryId}.");
                }

                var result = _mapper.Map<List<ProductDto>>(products);
                _logger.LogInformation($"Đã lấy {result.Count} sản phẩm thuộc danh mục {categoryId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy danh sách sản phẩm thuộc danh mục {categoryId}", ex);
                throw;
            }
        }
        public async Task<List<ProductDto>> SearchTemp(string keyword)
        {
            try
            {
                string sql = @"
                        SELECT a.* 
                        FROM PRODUCT a 
                        LEFT JOIN PRODUCT_CATEGORY b ON a.category_id = b.category_id 
                        left join BRAND c on a.brand_id =c.brand_id 
                        WHERE a.product_name LIKE @p0 OR b.category_name LIKE @p0 OR c.brand_name LIKE @p0";

                string likeKeyword = $"%{keyword?.Trim()}%";
                var list = await _repo.SqlQuery<Product>(sql, likeKeyword);
                var result = _mapper.Map<List<ProductDto>>(list);
                _logger.LogInformation($"Found {result.Count} products matching keyword '{keyword}'");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in SearchTemp with keyword: {keyword}", ex);
                throw;
            }
        }


    }
}
