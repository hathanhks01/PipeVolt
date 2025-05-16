using AutoMapper;
using PipeVolt_Api.Common;
using PipeVolt_Api.Common.Repository;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _repo;
        private readonly IMapper _mapper;

        public ProductService(
            IGenericRepository<Product> repo,
            IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var products = await _repo.QueryBy(p => p.ProductId == productId);
            var product = await Task.Run(() => products.FirstOrDefault());
            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _repo.GetAll();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
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
                    throw new InvalidOperationException("Failed to upload image: " + ex.Message);
                }
            }

            var created = await _repo.Create(entity);
            return _mapper.Map<ProductDto>(created);
        }

        public async Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto dto)
        {
            // 1. Lấy entity
            var products = await _repo.QueryBy(p => p.ProductId == productId);
            var entity = await Task.Run(() => products.FirstOrDefault());
            if (entity == null)
                throw new KeyNotFoundException("Product not found.");

            // 2. Map dto lên entity
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
                    throw new InvalidOperationException("Failed to upload image: " + ex.Message);
                }
            }
            // 3. Cập nhật
            await _repo.Update(entity);

            return _mapper.Map<ProductDto>(entity);
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            // 1. Lấy entity
            var products = await _repo.QueryBy(p => p.ProductId == productId);
            var entity = await Task.Run(() => products.FirstOrDefault());
            if (entity == null)
                throw new KeyNotFoundException("Product not found.");

            // 2. Xóa
            await _repo.Delete(entity);
            return true;
        }
    }
}
