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
    public class SupplierService : ISupplierService
    {
        private readonly IGenericRepository<Supplier> _repo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public SupplierService(
            IGenericRepository<Supplier> repo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<SupplierDto>> GetAllSuppliersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all suppliers");
                var data = await _repo.GetAll();
                var result = _mapper.Map<List<SupplierDto>>(data);
                _logger.LogInformation($"Fetched {result.Count} suppliers");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching suppliers", ex);
                throw;
            }
        }

        public async Task<SupplierDto> GetSupplierByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching supplier ID {id}");
                var list = await _repo.QueryBy(x => x.SupplierId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Supplier ID {id} not found");
                    throw new KeyNotFoundException("Supplier not found.");
                }
                return _mapper.Map<SupplierDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching supplier ID {id}", ex);
                throw;
            }
        }

        public async Task<SupplierDto> AddSupplierAsync(CreateSupplierDto dto)
        {
            try
            {
                _logger.LogInformation("Adding new supplier");
                var entity = _mapper.Map<Supplier>(dto);
                var created = await _repo.Create(entity);
                var result = _mapper.Map<SupplierDto>(created);
                _logger.LogInformation($"Added supplier ID {result.SupplierId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding supplier", ex);
                throw;
            }
        }

        public async Task<SupplierDto> UpdateSupplierAsync(int id, UpdateSupplierDto dto)
        {
            if (dto == null || dto.SupplierId != id)
            {
                _logger.LogWarning("Invalid update request for supplier");
                throw new ArgumentException("Supplier ID mismatch.");
            }

            try
            {
                _logger.LogInformation($"Updating supplier ID {id}");
                var list = await _repo.QueryBy(x => x.SupplierId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Supplier ID {id} not found");
                    throw new KeyNotFoundException("Supplier not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<SupplierDto>(entity);
                _logger.LogInformation($"Updated supplier ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating supplier ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting supplier ID {id}");
                var list = await _repo.QueryBy(x => x.SupplierId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Supplier ID {id} not found");
                    throw new KeyNotFoundException("Supplier not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted supplier ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting supplier ID {id}", ex);
                throw;
            }
        }
    }
}
