using AutoMapper;
using PipeVolt_Api.Common;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IGenericRepository<ProductCategory> _repo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public ProductCategoryService(
            IGenericRepository<ProductCategory> repo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<List<ProductCategoryDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all product categories");
            var entities = await _repo.GetAll();
            return _mapper.Map<List<ProductCategoryDto>>(entities);
        }

        public async Task<ProductCategoryDto> GetByIdAsync(int id)
        {
            _logger.LogInformation($"Fetching category {id}");
            var query = await _repo.QueryBy(c => c.CategoryId == id);
            var entity = query.FirstOrDefault();
            if (entity == null)
            {
                _logger.LogWarning($"Category {id} not found");
                throw new KeyNotFoundException("Category not found");
            }
            return _mapper.Map<ProductCategoryDto>(entity);
        }

        public async Task<ProductCategoryDto> CreateAsync(CreateProductCategoryDto dto)
        {
            _logger.LogInformation($"Creating category {dto.CategoryName}");
            var entity = _mapper.Map<ProductCategory>(dto);
            if (dto.ImageFile != null)
            {
                try
                {
                    string imagePath = CommonFunctions.UploadFile(dto.ImageFile, "images/categories");
                    entity.ImageUrl = imagePath;
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError("Image upload failed", ex);
                    throw new InvalidOperationException("Failed to upload image: " + ex.Message);
                }
            }
            var created = await _repo.Create(entity);
            return _mapper.Map<ProductCategoryDto>(created);
        }

        public async Task<ProductCategoryDto> UpdateAsync(int id, UpdateProductCategoryDto dto)
        {
            if (id != dto.CategoryId)
                throw new ArgumentException("ID mismatch");
            var query = await _repo.QueryBy(c => c.CategoryId == id);
            var entity = query.FirstOrDefault();
            if (entity == null) throw new KeyNotFoundException("Category not found");
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
            return _mapper.Map<ProductCategoryDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = await _repo.QueryBy(c => c.CategoryId == id);
            var entity = query.FirstOrDefault();
            if (entity == null) throw new KeyNotFoundException("Category not found");
            await _repo.Delete(entity);
            return true;
        }
    }
}
