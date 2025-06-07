using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using PipeVolt_DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class BrandService : IBrandService
    {
        private readonly IGenericRepository<Brand> _repo;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public BrandService(IGenericRepository<Brand> repo, ILoggerService loggerService, IMapper mapper)
        {
            _repo = repo;
            _loggerService = loggerService;
            _mapper = mapper; 
        }

        public async Task<List<BrandDto>> GetAllBrandsAsync()
        {
            try
            {
                var brands = await _repo.GetAll();
                _loggerService.LogInformation("Fetched all brands successfully");
                return _mapper.Map<List<BrandDto>>(brands);
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Error fetching all brands", ex);
                throw;
            }
        }
        public async Task<BrandDto> GetBrandByIdAsync(int id)
        {
            try
            {
                var brand = await _repo.QueryBy(p => p.BrandId == id);
                var entity = await Task.Run(() => brand.FirstOrDefault());
                if (entity == null)
                    throw new KeyNotFoundException("brand not found.");
                return _mapper.Map<BrandDto>(brand); 
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error fetching brand with ID {id}", ex);
                throw;
            }
        }
        public async Task<BrandDto> AddBrandAsync(BrandDto brand)
        {
            try
            {
                var entity = _mapper.Map<Brand>(brand);
                await _repo.Create(entity);
                _loggerService.LogInformation($"Brand with ID {entity.BrandId} added successfully");
                return _mapper.Map<BrandDto>(entity);
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Error adding brand", ex);
                throw;
            }
        }
        public async Task<BrandDto> UpdateBrandAsync(int id,BrandDto brand)
        {
            try
            {
                var existingBrand = await _repo.QueryBy(p => p.BrandId == id);
                var entity = await Task.Run(() => existingBrand.FirstOrDefault());
                if (entity == null)
                    throw new KeyNotFoundException("brand not found.");
                _mapper.Map(brand, entity);
                await _repo.Update(entity);
                _loggerService.LogInformation($"Brand with ID {id} updated successfully");
                return _mapper.Map<BrandDto>(entity);
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Error updating brand", ex);
                throw;
            }
        }
        public async Task<bool> DeleteProductAsync(int BrandId)
        {
            try
            {
                var products = await _repo.QueryBy(p => p.BrandId == BrandId);
                var entity = await Task.Run(() => products.FirstOrDefault());
                if (entity == null)
                    throw new KeyNotFoundException("Brand not found.");

                // 2. Xóa
                await _repo.Delete(entity);
                _loggerService.LogInformation($"Brand with ID {BrandId} deleted successfully");

                return true; 
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Error delete brand",ex);
                return false;
            }
           
        }
    }
}
