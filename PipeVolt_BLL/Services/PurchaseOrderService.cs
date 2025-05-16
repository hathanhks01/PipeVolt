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
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IGenericRepository<PurchaseOrder> _repo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public PurchaseOrderService(
            IGenericRepository<PurchaseOrder> repo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<PurchaseOrderDto>> GetAllPurchaseOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all purchase orders");
                var entities = await _repo.GetAll();
                var result = _mapper.Map<List<PurchaseOrderDto>>(entities);
                _logger.LogInformation($"Fetched {result.Count} purchase orders");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching purchase orders", ex);
                throw;
            }
        }

        public async Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching purchase order ID {id}");
                var list = await _repo.QueryBy(x => x.PurchaseOrderId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Purchase order ID {id} not found");
                    throw new KeyNotFoundException("Purchase order not found.");
                }
                var dto = _mapper.Map<PurchaseOrderDto>(entity);
                _logger.LogInformation($"Fetched purchase order ID {id}");
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching purchase order ID {id}", ex);
                throw;
            }
        }

        public async Task<PurchaseOrderDto> AddPurchaseOrderAsync(CreatePurchaseOrderDto dto)
        {
            try
            {
                _logger.LogInformation("Adding new purchase order");
                var entity = _mapper.Map<PurchaseOrder>(dto);
                var created = await _repo.Create(entity);
                var result = _mapper.Map<PurchaseOrderDto>(created);
                _logger.LogInformation($"Added purchase order ID {result.PurchaseOrderId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding purchase order", ex);
                throw;
            }
        }

        public async Task<PurchaseOrderDto> UpdatePurchaseOrderAsync(int id, UpdatePurchaseOrderDto dto)
        {
            if (dto == null || dto.PurchaseOrderId != id)
            {
                _logger.LogWarning("Invalid update request for purchase order");
                throw new ArgumentException("PurchaseOrder ID mismatch.");
            }

            try
            {
                _logger.LogInformation($"Updating purchase order ID {id}");
                var list = await _repo.QueryBy(x => x.PurchaseOrderId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Purchase order ID {id} not found");
                    throw new KeyNotFoundException("Purchase order not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<PurchaseOrderDto>(entity);
                _logger.LogInformation($"Updated purchase order ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating purchase order ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeletePurchaseOrderAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting purchase order ID {id}");
                var list = await _repo.QueryBy(x => x.PurchaseOrderId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Purchase order ID {id} not found");
                    throw new KeyNotFoundException("Purchase order not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted purchase order ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting purchase order ID {id}", ex);
                throw;
            }
        }
    }
}
