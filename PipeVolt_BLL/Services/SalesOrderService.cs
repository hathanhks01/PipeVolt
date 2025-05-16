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
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IGenericRepository<SalesOrder> _repo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public SalesOrderService(
            IGenericRepository<SalesOrder> repo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<SalesOrderDto>> GetAllSalesOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all sales orders");
                var data = await _repo.GetAll();
                var result = _mapper.Map<List<SalesOrderDto>>(data);
                _logger.LogInformation($"Fetched {result.Count} sales orders");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching sales orders", ex);
                throw;
            }
        }

        public async Task<SalesOrderDto> GetSalesOrderByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching sales order ID {id}");
                var list = await _repo.QueryBy(x => x.OrderId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Sales order ID {id} not found");
                    throw new KeyNotFoundException("Sales order not found.");
                }
                return _mapper.Map<SalesOrderDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching sales order ID {id}", ex);
                throw;
            }
        }

        public async Task<SalesOrderDto> AddSalesOrderAsync(CreateSalesOrderDto dto)
        {
            try
            {
                _logger.LogInformation("Adding new sales order");
                var entity = _mapper.Map<SalesOrder>(dto);
                var created = await _repo.Create(entity);
                var result = _mapper.Map<SalesOrderDto>(created);
                _logger.LogInformation($"Added sales order ID {result.OrderId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding sales order", ex);
                throw;
            }
        }

        public async Task<SalesOrderDto> UpdateSalesOrderAsync(int id, UpdateSalesOrderDto dto)
        {
            if (dto == null || dto.OrderId != id)
            {
                _logger.LogWarning("Invalid update request for sales order");
                throw new ArgumentException("SalesOrder ID mismatch.");
            }

            try
            {
                _logger.LogInformation($"Updating sales order ID {id}");
                var list = await _repo.QueryBy(x => x.OrderId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Sales order ID {id} not found");
                    throw new KeyNotFoundException("Sales order not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<SalesOrderDto>(entity);
                _logger.LogInformation($"Updated sales order ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating sales order ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteSalesOrderAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting sales order ID {id}");
                var list = await _repo.QueryBy(x => x.OrderId == id);
                var entity = list.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Sales order ID {id} not found");
                    throw new KeyNotFoundException("Sales order not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted sales order ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting sales order ID {id}", ex);
                throw;
            }
        }
    }
}
