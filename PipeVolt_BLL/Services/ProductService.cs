using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public ProductService(
            IGenericRepository<Product> repo,
            IMapper mapper,
            ILoggerService logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            try
            {
                var products = await _repo.QueryBy(p => p.ProductId == productId);
                var product = await Task.Run(() => products.FirstOrDefault());
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

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                var products = await _repo.GetAll();
                return _mapper.Map<IEnumerable<ProductDto>>(products);
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
                _logger.LogError("Error in GetAllProductsAsync", ex);
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

    }
}
