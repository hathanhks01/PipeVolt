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
    public class WarrantyService : IWarrantyService
    {
        private readonly IGenericRepository<Warranty> _repo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public WarrantyService(
            IGenericRepository<Warranty> repo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<WarrantyDto>> GetAllWarrantiesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all warranties");
                var data = await _repo.GetAll();
                var result = _mapper.Map<List<WarrantyDto>>(data);
                _logger.LogInformation($"Fetched {result.Count} warranties");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching warranties", ex);
                throw;
            }
        }

        public async Task<WarrantyDto> GetWarrantyByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching warranty ID {id}");
                var list = await _repo.QueryBy(x => x.WarrantyId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Warranty ID {id} not found");
                    throw new KeyNotFoundException("Warranty not found.");
                }
                return _mapper.Map<WarrantyDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching warranty ID {id}", ex);
                throw;
            }
        }

        public async Task<WarrantyDto> AddWarrantyAsync(CreateWarrantyDto dto)
        {
            try
            {
                _logger.LogInformation("Adding new warranty");
                var entity = _mapper.Map<Warranty>(dto);
                var created = await _repo.Create(entity);
                var result = _mapper.Map<WarrantyDto>(created);
                _logger.LogInformation($"Added warranty ID {result.WarrantyId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding warranty", ex);
                throw;
            }
        }

        public async Task<WarrantyDto> UpdateWarrantyAsync(int id, UpdateWarrantyDto dto)
        {
            if (dto == null || dto.WarrantyId != id)
            {
                _logger.LogWarning("Invalid update request for warranty");
                throw new ArgumentException("Warranty ID mismatch.");
            }

            try
            {
                _logger.LogInformation($"Updating warranty ID {id}");
                var list = await _repo.QueryBy(x => x.WarrantyId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Warranty ID {id} not found");
                    throw new KeyNotFoundException("Warranty not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<WarrantyDto>(entity);
                _logger.LogInformation($"Updated warranty ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating warranty ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteWarrantyAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting warranty ID {id}");
                var list = await _repo.QueryBy(x => x.WarrantyId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Warranty ID {id} not found");
                    throw new KeyNotFoundException("Warranty not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted warranty ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting warranty ID {id}", ex);
                throw;
            }
        }
    }
}
