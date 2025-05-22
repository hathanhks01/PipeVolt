using AutoMapper;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using PipeVolt_DAL.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IGenericRepository<Cart> _cartGenericRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IGenericRepository<CartItem> _cartItemGenericRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Inventory> _inventoryRepository;
        private readonly IGenericRepository<SalesOrder> _salesOrderRepository;
        private readonly IGenericRepository<OrderDetail> _orderDetailRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public CartService(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IGenericRepository<Product> productRepository,
            IGenericRepository<Inventory> inventoryRepository,
            IGenericRepository<SalesOrder> salesOrderRepository,
            IGenericRepository<OrderDetail> orderDetailRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _cartItemRepository = cartItemRepository ?? throw new ArgumentNullException(nameof(cartItemRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
            _salesOrderRepository = salesOrderRepository ?? throw new ArgumentNullException(nameof(salesOrderRepository));
            _orderDetailRepository = orderDetailRepository ?? throw new ArgumentNullException(nameof(orderDetailRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cartGenericRepository = cartRepository as IGenericRepository<Cart> ?? throw new ArgumentNullException(nameof(cartRepository));
            _cartItemGenericRepository = cartItemRepository as IGenericRepository<CartItem> ?? throw new ArgumentNullException(nameof(cartItemRepository));
        }

        public async Task<CartDto> GetCartAsync(int customerId)
        {
            try
            {
                _logger.LogInformation($"Fetching cart for customer {customerId}");
                var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    _logger.LogWarning($"Cart not found for customer {customerId}");
                    return new CartDto { CustomerId = customerId };
                }

                var cartDto = _mapper.Map<CartDto>(cart);
                cartDto.TotalAmount = cart.CartItems.Sum(ci => ci.LineTotal);
                _logger.LogInformation($"Fetched cart ID {cartDto.CartId} for customer {customerId}");
                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching cart for customer {customerId}", ex);
                throw;
            }
        }

        public async Task<CartDto> AddCartAsync(CreateCartDto dto)
        {
            try
            {
                _logger.LogInformation($"Adding new cart for customer {dto.CustomerId}");
                var existingCart = await _cartRepository.GetCartByCustomerIdAsync(dto.CustomerId);
                if (existingCart != null)
                {
                    _logger.LogWarning($"Cart already exists for customer {dto.CustomerId}");
                    throw new InvalidOperationException("Cart already exists for this customer.");
                }

                var entity = _mapper.Map<Cart>(dto);
                var created = await _cartGenericRepository.Create(entity);
                var result = _mapper.Map<CartDto>(created);
                _logger.LogInformation($"Added cart ID {result.CartId} for customer {dto.CustomerId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding cart for customer {dto.CustomerId}", ex);
                throw;
            }
        }

        public async Task<CartDto> UpdateCartAsync(int id, UpdateCartDto dto)
        {
            if (dto == null || dto.CartId != id)
            {
                _logger.LogWarning("Invalid update request for cart");
                throw new ArgumentException("Cart ID mismatch.");
            }

            try
            {
                _logger.LogInformation($"Updating cart ID {id}");
                var query = await _cartGenericRepository.QueryBy(x => x.CartId == id);
                var entity = query.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Cart ID {id} not found");
                    throw new KeyNotFoundException("Cart not found.");
                }

                _mapper.Map(dto, entity);
                entity.UpdatedAt = DateTime.Now;
                await _cartGenericRepository.Update(entity);
                var result = _mapper.Map<CartDto>(entity);
                _logger.LogInformation($"Updated cart ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating cart ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteCartAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting cart ID {id}");
                var query = await _cartGenericRepository.QueryBy(x => x.CartId == id);
                var entity = query.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Cart ID {id} not found");
                    throw new KeyNotFoundException("Cart not found.");
                }

                await _cartGenericRepository.Delete(entity);
                _logger.LogInformation($"Deleted cart ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting cart ID {id}", ex);
                throw;
            }
        }

        public async Task<CartDto> AddItemToCartAsync(int customerId, AddCartItemDto itemDto)
        {
            try
            {
                _logger.LogInformation($"Adding item to cart for customer {customerId}");
                var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    cart = await _cartGenericRepository.Create(new Cart { CustomerId = customerId });
                }

                var productQuery = await _productRepository.QueryBy(p => p.ProductId == itemDto.ProductId);
                var product = productQuery.FirstOrDefault();
                if (product == null)
                {
                    _logger.LogWarning($"Product {itemDto.ProductId} not found");
                    throw new KeyNotFoundException("Product not found.");
                }

                var inventoryQuery = await _inventoryRepository.QueryBy(i => i.ProductId == itemDto.ProductId);
                var inventory = inventoryQuery.FirstOrDefault();
                if (inventory == null || inventory.Quantity < itemDto.Quantity)
                {
                    _logger.LogWarning($"Insufficient stock for product {itemDto.ProductId}");
                    throw new InvalidOperationException("Insufficient stock.");
                }

                var cartItem = await _cartItemRepository.GetCartItemByCartAndProductAsync(cart.CartId, itemDto.ProductId);
                if (cartItem != null)
                {
                    cartItem.Quantity += itemDto.Quantity;
                    cartItem.UnitPrice = product.SellingPrice ?? 0;
                    await _cartItemGenericRepository.Update(cartItem); // Sửa lỗi: dùng _cartItemGenericRepository
                }
                else
                {
                    cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.SellingPrice ?? 0
                    };
                    await _cartItemGenericRepository.Create(cartItem); // Sửa lỗi: dùng _cartItemGenericRepository
                }

                cart.UpdatedAt = DateTime.Now;
                await _cartGenericRepository.Update(cart);
                var result = await GetCartAsync(customerId);
                _logger.LogInformation($"Added item to cart ID {cart.CartId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding item to cart for customer {customerId}", ex);
                throw;
            }
        }

        public async Task<CartDto> UpdateCartItemAsync(int customerId, UpdateCartItemDto itemDto)
        {
            try
            {
                _logger.LogInformation($"Updating cart item {itemDto.CartItemId} for customer {customerId}");
                var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    _logger.LogWarning($"Cart not found for customer {customerId}");
                    throw new KeyNotFoundException("Cart not found.");
                }

                var cartItemQuery = await _cartItemGenericRepository.QueryBy(ci => ci.CartItemId == itemDto.CartItemId && ci.CartId == cart.CartId);
                var cartItem = cartItemQuery.FirstOrDefault();
                if (cartItem == null)
                {
                    _logger.LogWarning($"Cart item {itemDto.CartItemId} not found");
                    throw new KeyNotFoundException("Cart item not found.");
                }

                var inventoryQuery = await _inventoryRepository.QueryBy(i => i.ProductId == cartItem.ProductId);
                var inventory = inventoryQuery.FirstOrDefault();
                if (inventory == null || inventory.Quantity < itemDto.Quantity)
                {
                    _logger.LogWarning($"Insufficient stock for product {cartItem.ProductId}");
                    throw new InvalidOperationException("Insufficient stock.");
                }

                cartItem.Quantity = itemDto.Quantity;
                await _cartItemGenericRepository.Update(cartItem);
                cart.UpdatedAt = DateTime.Now;
                await _cartGenericRepository.Update(cart); // Sửa lỗi: dùng _cartGenericRepository
                var result = await GetCartAsync(customerId);
                _logger.LogInformation($"Updated cart item {itemDto.CartItemId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating cart item {itemDto.CartItemId}", ex);
                throw;
            }
        }

        public async Task<CartDto> RemoveCartItemAsync(int customerId, int cartItemId)
        {
            try
            {
                _logger.LogInformation($"Removing cart item {cartItemId} for customer {customerId}");
                var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    _logger.LogWarning($"Cart not found for customer {customerId}");
                    throw new KeyNotFoundException("Cart not found.");
                }

                var cartItemQuery = await _cartItemGenericRepository.QueryBy(ci => ci.CartItemId == cartItemId && ci.CartId == cart.CartId);
                var cartItem = cartItemQuery.FirstOrDefault();
                if (cartItem == null)
                {
                    _logger.LogWarning($"Cart item {cartItemId} not found");
                    throw new KeyNotFoundException("Cart item not found.");
                }

                await _cartItemGenericRepository.Delete(cartItem);
                cart.UpdatedAt = DateTime.Now;
                await _cartGenericRepository.Update(cart); // Sửa lỗi: dùng _cartGenericRepository
                var result = await GetCartAsync(customerId);
                _logger.LogInformation($"Removed cart item {cartItemId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing cart item {cartItemId}", ex);
                throw;
            }
        }

        public async Task<int> CheckoutAsync(int customerId)
        {
            try
            {
                _logger.LogInformation($"Checking out for customer {customerId}");
                var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
                if (cart == null || !cart.CartItems.Any())
                {
                    _logger.LogWarning($"Cart is empty or not found for customer {customerId}");
                    throw new InvalidOperationException("Cart is empty.");
                }

                foreach (var item in cart.CartItems)
                {
                    var inventoryQuery = await _inventoryRepository.QueryBy(i => i.ProductId == item.ProductId);
                    var inventory = inventoryQuery.FirstOrDefault();
                    if (inventory == null || inventory.Quantity < item.Quantity)
                    {
                        _logger.LogWarning($"Insufficient stock for product {item.ProductId}");
                        throw new InvalidOperationException($"Insufficient stock for product {item.Product.ProductName}");
                    }
                }

                var salesOrder = new SalesOrder
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    PaymentMethod = "Cash", // Có thể thay đổi
                    OrderCode = $"SO-{DateTime.Now.Ticks}"
                };

                var createdOrder = await _salesOrderRepository.Create(salesOrder);

                foreach (var item in cart.CartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = createdOrder.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    await _orderDetailRepository.Create(orderDetail);

                    var inventoryQuery = await _inventoryRepository.QueryBy(i => i.ProductId == item.ProductId);
                    var inventory = inventoryQuery.FirstOrDefault();
                    inventory.Quantity -= item.Quantity;
                    inventory.UpdatedAt = DateTime.Now;
                    await _inventoryRepository.Update(inventory);
                }

                await _cartItemGenericRepository.DeleteRange(ci => ci.CartId == cart.CartId);
                await _cartGenericRepository.Delete(cart);

                _logger.LogInformation($"Checkout successful for customer {customerId}, order ID {createdOrder.OrderId}");
                return createdOrder.OrderId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during checkout for customer {customerId}", ex);
                throw;
            }
        }
    }
}