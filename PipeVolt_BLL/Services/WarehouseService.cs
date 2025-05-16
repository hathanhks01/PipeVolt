using AutoMapper;
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
    public class WarehouseService : IWarehouseService
    {
        private readonly IGenericRepository<Warehouse> _repo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public WarehouseService(
            IGenericRepository<Warehouse> repo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<WarehouseDto>> GetAllWarehousesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all warehouses");
                var data = await _repo.GetAll();
                var result = _mapper.Map<List<WarehouseDto>>(data);
                _logger.LogInformation($"Fetched {result.Count} warehouses");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching warehouses", ex);
                throw;
            }
        }

        public async Task<WarehouseDto> GetWarehouseByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching warehouse ID {id}");
                var list = await _repo.QueryBy(x => x.WarehouseId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Warehouse ID {id} not found");
                    throw new KeyNotFoundException("Warehouse not found.");
                }
                return _mapper.Map<WarehouseDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching warehouse ID {id}", ex);
                throw;
            }
        }

        public async Task<WarehouseDto> AddWarehouseAsync(CreateWarehouseDto dto)
        {
            try
            {
                _logger.LogInformation("Adding new warehouse");
                var entity = _mapper.Map<Warehouse>(dto);
                var created = await _repo.Create(entity);
                var result = _mapper.Map<WarehouseDto>(created);
                _logger.LogInformation($"Added warehouse ID {result.WarehouseId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding warehouse", ex);
                throw;
            }
        }

        public async Task<WarehouseDto> UpdateWarehouseAsync(int id, UpdateWarehouseDto dto)
        {
            if (dto == null || dto.WarehouseId != id)
            {
                _logger.LogWarning("Invalid update request for warehouse");
                throw new ArgumentException("Warehouse ID mismatch.");
            }

            try
            {
                _logger.LogInformation($"Updating warehouse ID {id}");
                var list = await _repo.QueryBy(x => x.WarehouseId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Warehouse ID {id} not found");
                    throw new KeyNotFoundException("Warehouse not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<WarehouseDto>(entity);
                _logger.LogInformation($"Updated warehouse ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating warehouse ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteWarehouseAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting warehouse ID {id}");
                var list = await _repo.QueryBy(x => x.WarehouseId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Warehouse ID {id} not found");
                    throw new KeyNotFoundException("Warehouse not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted warehouse ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting warehouse ID {id}", ex);
                throw;
            }
        }
    }
}
