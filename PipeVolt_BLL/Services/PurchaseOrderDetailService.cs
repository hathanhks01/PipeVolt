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
    public class PurchaseOrderDetailService : IPurchaseOrderDetailService
    {
        private readonly IGenericRepository<PurchaseOrderDetail> _repo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public PurchaseOrderDetailService(
            IGenericRepository<PurchaseOrderDetail> repo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<PurchaseOrderDetailDto>> GetAllPurchaseOrderDetailsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all purchase order details");
                var data = await _repo.GetAll();
                var result = _mapper.Map<List<PurchaseOrderDetailDto>>(data);
                _logger.LogInformation($"Fetched {result.Count} purchase order details");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching purchase order details", ex);
                throw;
            }
        }

        public async Task<PurchaseOrderDetailDto> GetPurchaseOrderDetailByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching purchase order detail ID {id}");
                var list = await _repo.QueryBy(x => x.PurchaseOrderDetailId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Purchase order detail ID {id} not found");
                    throw new KeyNotFoundException("Purchase order detail not found.");
                }
                return _mapper.Map<PurchaseOrderDetailDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching purchase order detail ID {id}", ex);
                throw;
            }
        }

        public async Task<PurchaseOrderDetailDto> AddPurchaseOrderDetailAsync(CreatePurchaseOrderDetailDto dto)
        {
            try
            {
                _logger.LogInformation("Adding new purchase order detail");
                var entity = _mapper.Map<PurchaseOrderDetail>(dto);
                var created = await _repo.Create(entity);
                var result = _mapper.Map<PurchaseOrderDetailDto>(created);
                _logger.LogInformation($"Added purchase order detail ID {result.PurchaseOrderDetailId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding purchase order detail", ex);
                throw;
            }
        }

        public async Task<PurchaseOrderDetailDto> UpdatePurchaseOrderDetailAsync(int id, UpdatePurchaseOrderDetailDto dto)
        {
            if (dto == null || dto.PurchaseOrderDetailId != id)
            {
                _logger.LogWarning("Invalid update request for purchase order detail");
                throw new ArgumentException("ID mismatch");
            }

            try
            {
                _logger.LogInformation($"Updating purchase order detail ID {id}");
                var list = await _repo.QueryBy(x => x.PurchaseOrderDetailId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Purchase order detail ID {id} not found");
                    throw new KeyNotFoundException("Purchase order detail not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<PurchaseOrderDetailDto>(entity);
                _logger.LogInformation($"Updated purchase order detail ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating purchase order detail ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeletePurchaseOrderDetailAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting purchase order detail ID {id}");
                var list = await _repo.QueryBy(x => x.PurchaseOrderDetailId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Purchase order detail ID {id} not found");
                    throw new KeyNotFoundException("Purchase order detail not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted purchase order detail ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting purchase order detail ID {id}", ex);
                throw;
            }
        }
    }
}
