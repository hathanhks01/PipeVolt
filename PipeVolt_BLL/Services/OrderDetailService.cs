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
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IGenericRepository<OrderDetail> _repo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public OrderDetailService(
            IGenericRepository<OrderDetail> repo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<OrderDetailDto>> GetAllOrderDetailsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all order details");
                var data = await _repo.GetAll();
                var result = _mapper.Map<List<OrderDetailDto>>(data);
                _logger.LogInformation($"Fetched {result.Count} order details");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching order details", ex);
                throw;
            }
        }

        public async Task<OrderDetailDto> GetOrderDetailByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching order detail with ID {id}");
                var entities = await _repo.QueryBy(x => x.OrderDetailId == id);
                var entity = entities.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Order detail with ID {id} not found");
                    throw new KeyNotFoundException("Order detail not found.");
                }
                return _mapper.Map<OrderDetailDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching order detail with ID {id}", ex);
                throw;
            }
        }

        public async Task<OrderDetailDto> AddOrderDetailAsync(CreateOrderDetailDto dto)
        {
            try
            {
                _logger.LogInformation("Adding new order detail");
                var entity = _mapper.Map<OrderDetail>(dto);
                var created = await _repo.Create(entity);
                var result = _mapper.Map<OrderDetailDto>(created);
                _logger.LogInformation($"Added order detail with ID {result.OrderDetailId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding order detail", ex);
                throw;
            }
        }

        public async Task<OrderDetailDto> UpdateOrderDetailAsync(int id, UpdateOrderDetailDto dto)
        {
            if (dto == null || dto.OrderDetailId != id)
            {
                _logger.LogWarning("Invalid update request for order detail");
                throw new ArgumentException("Invalid update data.");
            }

            try
            {
                _logger.LogInformation($"Updating order detail with ID {id}");
                var entities = await _repo.QueryBy(x => x.OrderDetailId == id);
                var entity = entities.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Order detail with ID {id} not found");
                    throw new KeyNotFoundException("Order detail not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<OrderDetailDto>(entity);
                _logger.LogInformation($"Updated order detail with ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating order detail with ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteOrderDetailAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting order detail with ID {id}");
                var entities = await _repo.QueryBy(x => x.OrderDetailId == id);
                var entity = entities.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Order detail with ID {id} not found");
                    throw new KeyNotFoundException("Order detail not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted order detail with ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting order detail with ID {id}", ex);
                throw;
            }
        }
    }
}
