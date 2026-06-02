using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PipeVolt_Api.Common;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.Common.Repository;
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
        private readonly IGenericRepository<Inventory> _inventoryRepo;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        private readonly IMemoryCache _cache;
        private readonly ICacheService _cacheService;

        public ProductService(
            IGenericRepository<Product> repo,
            IGenericRepository<ProductCategory> categoryRepo,
            IGenericRepository<Inventory> inventoryRepo,
            IMapper mapper,
            ILoggerService logger,
            IMemoryCache cache,
            ICacheService cacheService)
        {
            _repo = repo;
            _categoryRepo = categoryRepo ?? throw new ArgumentNullException(nameof(categoryRepo));
            _inventoryRepo = inventoryRepo ?? throw new ArgumentNullException(nameof(inventoryRepo));
            _mapper = mapper;
            _logger = logger;
            _cacheService = cacheService;
            _cache = cache;
        }

        /// <summary>
        /// Populate quantity vào danh sách ProductDto từ bảng Inventory
        /// (cột quantity đã bị drop khỏi bảng PRODUCT)
        /// </summary>
        private async Task PopulateInventoryQuantitiesAsync(List<ProductDto> dtos)
        {
            if (dtos == null || !dtos.Any()) return;

            var productIds = dtos.Select(p => p.ProductId).ToList();
            var inventories = await _inventoryRepo.QueryBy(i => productIds.Contains(i.ProductId));
            var inventoryList = await inventories.ToListAsync();

            foreach (var dto in dtos)
            {
                dto.quantity = inventoryList
                    .Where(i => i.ProductId == dto.ProductId)
                    .Sum(i => i.Quantity);
            }
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            try
            {
                var products = await _repo.QueryBy(p => p.ProductId == productId);
                var product = await products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync();

                if (product == null)
                    throw new KeyNotFoundException("Product not found.");

                var dto = _mapper.Map<ProductDto>(product);

                // Lấy quantity từ bảng Inventory thay vì cột đã drop
                var inventories = await _inventoryRepo.QueryBy(i => i.ProductId == productId);
                var inventoryList = await inventories.ToListAsync();
                dto.quantity = inventoryList.Sum(i => i.Quantity);

                return dto;
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
                var product = await products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync();

                if (product == null)
                    throw new KeyNotFoundException("Product not found.");

                // Lấy các sản phẩm liên quan: cùng CategoryId, khác ProductId hiện tại, tối đa 5
                List<Product> relatedProducts = new List<Product>();
                if (product.CategoryId.HasValue)
                {
                    var relatedQuery = await _repo.QueryBy(p =>
                        p.CategoryId == product.CategoryId &&
                        p.ProductId != product.ProductId);
                    relatedProducts = await relatedQuery
                        .Include(p => p.Brand)
                        .Include(p => p.Category)
                        .Take(5)
                        .ToListAsync();
                }

                var productDto = _mapper.Map<ProductDto>(product);

                // Lấy quantity từ Inventory cho sản phẩm chính
                var mainInventories = await _inventoryRepo.QueryBy(i => i.ProductId == productId);
                productDto.quantity = (await mainInventories.ToListAsync()).Sum(i => i.Quantity);

                // Map related products và populate quantity từ Inventory
                var relatedDtos = _mapper.Map<List<ProductDto>>(relatedProducts);
                await PopulateInventoryQuantitiesAsync(relatedDtos);

                if (productDto != null)
                    productDto.RelatedProducts = relatedDtos;

                return productDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetProductByIdAndRelatedAsync", ex);
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            return await _cacheService.GetOrSetAsync(
                CacheKeys.AllProducts,
                async () =>
                {
                    try
                    {
                        return await GetAllProductsWithInventoryAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error in GetAllProductsAsync (cache factory)", ex);
                        return Enumerable.Empty<ProductDto>();
                    }
                },
                TimeSpan.FromMinutes(30)
            );
        }

        /// <summary>
        /// Lấy tất cả sản phẩm kèm quantity tổng hợp từ bảng Inventory
        /// </summary>
        private async Task<IEnumerable<ProductDto>> GetAllProductsWithInventoryAsync()
        {
            var products = await _repo.QueryBy(p => true);
            var productList = await products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .ToListAsync();

            // Lấy toàn bộ inventory một lần, group theo ProductId
            var allInventories = await _inventoryRepo.GetAll();
            var inventoryLookup = allInventories
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            var dtos = _mapper.Map<List<ProductDto>>(productList);
            foreach (var dto in dtos)
            {
                dto.quantity = inventoryLookup.TryGetValue(dto.ProductId, out var qty) ? qty : 0;
            }

            return dtos;
        }

        public async Task<IEnumerable<ProductDto>> GetPopularProductsAsync()
        {
            try
            {
                var products = await _repo.QueryBy(p => true);
                var productList = await products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .OrderBy(p => p.ProductName)
                    .Take(5)
                    .ToListAsync();

                var dtos = _mapper.Map<List<ProductDto>>(productList);
                await PopulateInventoryQuantitiesAsync(dtos);

                return dtos;
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
                _cacheService.Remove(CacheKeys.AllProducts);

                var resultDto = _mapper.Map<ProductDto>(created);
                // Sản phẩm mới tạo chưa có trong Inventory => quantity = 0
                resultDto.quantity = 0;

                return resultDto;
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
                _cacheService.Remove(CacheKeys.AllProducts);

                var resultDto = _mapper.Map<ProductDto>(entity);

                // Populate quantity từ Inventory sau khi update
                var inventories = await _inventoryRepo.QueryBy(i => i.ProductId == productId);
                resultDto.quantity = (await inventories.ToListAsync()).Sum(i => i.Quantity);

                return resultDto;
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
                    }
                }

                await _repo.Delete(entity);
                _cacheService.Remove(CacheKeys.AllProducts);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in DeleteProductAsync", ex);
                throw;
            }
        }

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

                var dtos = _mapper.Map<List<ProductDto>>(products);

                // Populate quantity từ Inventory
                await PopulateInventoryQuantitiesAsync(dtos);

                _logger.LogInformation($"Đã lấy {dtos.Count} sản phẩm thuộc danh mục {categoryId}");
                return dtos;
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
                // Dùng LINQ thay raw SQL vì cột quantity đã bị drop khỏi PRODUCT
                var products = await _repo.QueryBy(p =>
                    p.ProductName.Contains(keyword) ||
                    (p.Category != null && p.Category.CategoryName.Contains(keyword)) ||
                    (p.Brand != null && p.Brand.BrandName.Contains(keyword)));

                var productList = await products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .ToListAsync();

                var dtos = _mapper.Map<List<ProductDto>>(productList);

                // Populate quantity từ Inventory
                await PopulateInventoryQuantitiesAsync(dtos);

                _logger.LogInformation($"Found {dtos.Count} products matching keyword '{keyword}'");
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in SearchTemp with keyword: {keyword}", ex);
                throw;
            }
        }
    }
}