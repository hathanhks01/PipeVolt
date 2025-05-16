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
    public class SupplyService : ISupplyService
    {
        private readonly IGenericRepository<Supply> _repo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public SupplyService(
            IGenericRepository<Supply> repo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<SupplyDto>> GetAllSuppliesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all supplies");
                var data = await _repo.GetAll();
                var result = _mapper.Map<List<SupplyDto>>(data);
                _logger.LogInformation($"Fetched {result.Count} supplies");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching supplies", ex);
                throw;
            }
        }

        public async Task<SupplyDto> GetSupplyByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching supply ID {id}");
                var list = await _repo.QueryBy(x => x.SupplyId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Supply ID {id} not found");
                    throw new KeyNotFoundException("Supply not found.");
                }
                return _mapper.Map<SupplyDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching supply ID {id}", ex);
                throw;
            }
        }

        public async Task<SupplyDto> AddSupplyAsync(CreateSupplyDto dto)
        {
            try
            {
                _logger.LogInformation("Adding new supply");
                var entity = _mapper.Map<Supply>(dto);
                var created = await _repo.Create(entity);
                var result = _mapper.Map<SupplyDto>(created);
                _logger.LogInformation($"Added supply ID {result.SupplyId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding supply", ex);
                throw;
            }
        }

        public async Task<SupplyDto> UpdateSupplyAsync(int id, UpdateSupplyDto dto)
        {
            if (dto == null || dto.SupplyId != id)
            {
                _logger.LogWarning("Invalid update request for supply");
                throw new ArgumentException("Supply ID mismatch.");
            }

            try
            {
                _logger.LogInformation($"Updating supply ID {id}");
                var list = await _repo.QueryBy(x => x.SupplyId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Supply ID {id} not found");
                    throw new KeyNotFoundException("Supply not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<SupplyDto>(entity);
                _logger.LogInformation($"Updated supply ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating supply ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteSupplyAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting supply ID {id}");
                var list = await _repo.QueryBy(x => x.SupplyId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Supply ID {id} not found");
                    throw new KeyNotFoundException("Supply not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted supply ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting supply ID {id}", ex);
                throw;
            }
        }
    }
}
