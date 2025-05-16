using AutoMapper;
using PipeVolt_Api.Common.Repository;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.IServices
{
    public class InventoryService : IInventoryService
    {
        private readonly IGenericRepository<Inventory> _repo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public InventoryService(
            IGenericRepository<Inventory> repo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<InventoryDto>> GetAllInventoriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all inventories");
                var inventories = await _repo.GetAll();
                var result = _mapper.Map<List<InventoryDto>>(inventories);
                _logger.LogInformation($"Fetched {result.Count} inventories");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching all inventories", ex);
                throw;
            }
        }

        public async Task<InventoryDto> GetInventoryByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching inventory with ID {id}");
                var inventories = await _repo.QueryBy(x => x.InventoryId == id);
                var entity = inventories.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Inventory with ID {id} not found");
                    throw new KeyNotFoundException("Inventory not found.");
                }
                var result = _mapper.Map<InventoryDto>(entity);
                _logger.LogInformation($"Fetched inventory with ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching inventory with ID {id}", ex);
                throw;
            }
        }

        public async Task<InventoryDto> AddInventoryAsync(CreateInventoryDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("Invalid inventory data: DTO is null");
                throw new ArgumentException("Invalid inventory data");
            }

            try
            {
                _logger.LogInformation("Adding new inventory");
                var entity = _mapper.Map<Inventory>(dto);
                var created = await _repo.Create(entity);
                var result = _mapper.Map<InventoryDto>(created);
                _logger.LogInformation($"Added inventory with ID {result.InventoryId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding inventory", ex);
                throw;
            }
        }

        public async Task<InventoryDto> UpdateInventoryAsync(int id, UpdateInventoryDto dto)
        {
            if (dto == null || dto.InventoryId != id)
            {
                _logger.LogWarning("Invalid inventory update: ID mismatch or DTO is null");
                throw new ArgumentException("Invalid inventory update request");
            }

            try
            {
                _logger.LogInformation($"Updating inventory with ID {id}");
                var inventories = await _repo.QueryBy(x => x.InventoryId == id);
                var entity = inventories.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Inventory with ID {id} not found");
                    throw new KeyNotFoundException("Inventory not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<InventoryDto>(entity);
                _logger.LogInformation($"Updated inventory with ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating inventory with ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting inventory with ID {id}");
                var inventories = await _repo.QueryBy(x => x.InventoryId == id);
                var entity = inventories.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Inventory with ID {id} not found");
                    throw new KeyNotFoundException("Inventory not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted inventory with ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting inventory with ID {id}", ex);
                throw;
            }
        }
    }
}
