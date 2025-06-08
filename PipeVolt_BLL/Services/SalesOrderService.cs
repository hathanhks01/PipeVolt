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
        public async Task Checkout(CheckoutDto checkoutDto)
        {
            if (checkoutDto == null || checkoutDto.Items == null || !checkoutDto.Items.Any())
            {
                _logger.LogWarning("Invalid checkout request: no items provided");
                throw new ArgumentException("Checkout items cannot be empty.");
            }

            _logger.LogInformation("Adding new sales order");

            // Tính tổng tiền
            double totalAmount = checkoutDto.Items.Sum(i => i.UnitPrice * i.Quantity);

            var salesOrder = new SalesOrder
            {
                CustomerId = checkoutDto.CustomerId,
                OrderDate = DateTime.UtcNow,
                Status = (int)SaleStatus.Pending,
                PaymentMethodId = checkoutDto.PaymentMethodId,
                TotalAmount = totalAmount,

            };

            await _repo.Create(salesOrder);
            _logger.LogInformation("Add new sales order success");

            foreach (var item in checkoutDto.Items)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = salesOrder.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                await _repoOrderDetail.Create(orderDetail);
            }

            var cartItemIds = checkoutDto.Items.Select(i => i.CartItemId).ToList();
            if (cartItemIds.Any())
            {
                _logger.LogInformation("Deleting cart items after checkout");
                var cartItemsQueryable = await _repoCartItem.QueryBy(x => cartItemIds.Contains(x.CartItemId));
                var cartItems = cartItemsQueryable.ToList(); // Đọc hết dữ liệu vào bộ nhớ, đóng DataReader

                foreach (var cartItem in cartItems)
                {
                    await _repoCartItem.Delete(cartItem);
                }
                _logger.LogInformation("Deleted cart items after checkout");
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
            var salesOrders = await _repo.QueryBy(x => x.CustomerId == userAccount.CustomerId.Value);
            var salesOrderList = salesOrders.ToList();

            _logger.LogInformation($"Fetched {salesOrderList.Count} sales orders for userId: {userId}");

            return _mapper.Map<List<SalesOrderDto>>(salesOrderList);
        }

        public async Task<IQueryable<SalesOrder>> QueryOrderWithDetails(int orderId)
        {
            _logger.LogInformation($"Querying sales order with details for OrderId: {orderId}");

            var query = await _repo.QueryBy(o => o.OrderId == orderId);
            var detailedQuery = query
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product);

            _logger.LogInformation($"Query built for OrderId: {orderId}");

            return detailedQuery;
        }
    }
}
