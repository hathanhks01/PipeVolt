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
using static PipeVolt_DAL.Common.DataType;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_BLL.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IGenericRepository<SalesOrder> _repo;
        private readonly IGenericRepository<OrderDetail> _repoOrderDetail;
        private readonly IGenericRepository<CartItem> _repoCartItem;
        private readonly IGenericRepository<UserAccount> _userAccountRepo; // thêm dòng này
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public SalesOrderService(
            IGenericRepository<SalesOrder> repo,
            IGenericRepository<CartItem> repoCartItem,
            IGenericRepository<OrderDetail> repoOrderDetail,
            IGenericRepository<UserAccount> userAccountRepo, // thêm dòng này
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _repoCartItem = repoCartItem ?? throw new ArgumentNullException(nameof(repoCartItem));
            _repoOrderDetail = repoOrderDetail ?? throw new ArgumentNullException(nameof(repoOrderDetail));
            _userAccountRepo = userAccountRepo ?? throw new ArgumentNullException(nameof(userAccountRepo)); // thêm dòng này
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<SalesOrderDto>> GetAllSalesOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all sales orders");
                var query = await _repo.QueryAll();
                var data = await query.Include(x => x.PaymentMethod).ToListAsync();
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
                var entity = await list.Include(x => x.PaymentMethod).FirstOrDefaultAsync();
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
                var createdQuery = await _repo.QueryBy(x => x.OrderId == created.OrderId);
                var createdWithNav = await createdQuery.Include(x => x.PaymentMethod).FirstOrDefaultAsync();
                var result = _mapper.Map<SalesOrderDto>(createdWithNav ?? created);
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
                var updatedQuery = await _repo.QueryBy(x => x.OrderId == id);
                var updatedWithNav = await updatedQuery.Include(x => x.PaymentMethod).FirstOrDefaultAsync();
                var result = _mapper.Map<SalesOrderDto>(updatedWithNav ?? entity);
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
        public async Task<List<SalesOrderDto>> GetSalesOrdersByUserIdAsync(int userId)
        {
            _logger.LogInformation($"Fetching sales orders for userId: {userId}");

            // Lấy UserAccount để tìm CustomerId
            var userAccounts = await _userAccountRepo.QueryBy(x => x.UserId == userId);
            var userAccount = userAccounts.FirstOrDefault();
            if (userAccount == null || userAccount.CustomerId == null)
            {
                _logger.LogWarning($"UserAccount with userId {userId} not found or does not have a CustomerId");
                return new List<SalesOrderDto>();
            }

            // Lấy các đơn hàng theo CustomerId
            var salesOrdersQuery = await _repo.QueryBy(x => x.CustomerId == userAccount.CustomerId.Value);
            var salesOrderList = await salesOrdersQuery.Include(x => x.PaymentMethod).ToListAsync();

            _logger.LogInformation($"Fetched {salesOrderList.Count} sales orders for userId: {userId}");

            return _mapper.Map<List<SalesOrderDto>>(salesOrderList);
        }

        public async Task<IQueryable<SalesOrder>> QueryOrderWithDetails(int orderId)
        {
            _logger.LogInformation($"Querying sales order with details for OrderId: {orderId}");

            var query = await _repo.QueryBy(o => o.OrderId == orderId);
            var detailedQuery = query
                .Include(o => o.Customer)
                .Include(o => o.PaymentMethod)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product);

            _logger.LogInformation($"Query built for OrderId: {orderId}");

            return detailedQuery;
        }

        public async Task<SalesOrder?> GetByOrderCodeAsync(string orderCode)
        {
            var query = await _repo.QueryBy(o => o.OrderCode == orderCode);
            return await query.FirstOrDefaultAsync();
        }
    }
}
