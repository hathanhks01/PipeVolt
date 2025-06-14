﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using PipeVolt_DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class CartItemService : ICartItemService
    {
        private readonly ICartItemRepository _cartItemRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<CartItem> _cartItemGenericRepo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        public CartItemService(
            ICartItemRepository cartItemRepo,
            ICartRepository cartRepo,
            IGenericRepository<Product> productRepository,
            IGenericRepository<CartItem> cartItemGenericRepo,
            ILoggerService logger,
            IMapper mapper,
            IMemoryCache cache)
        {
            _cartItemRepo = cartItemRepo ?? throw new ArgumentNullException(nameof(cartItemRepo));
            _cartRepo = cartRepo ?? throw new ArgumentNullException(nameof(cartRepo));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _cartItemGenericRepo = cartItemGenericRepo ?? throw new ArgumentNullException(nameof(cartItemGenericRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<IEnumerable<CartItemDto>> GetCartItemsByCartIdAsync(int cartId)
        {
            try
            {
                _logger.LogInformation($"Fetching cart items for CartId {cartId}");
                var items = await _cartItemGenericRepo.FindBy(ci => ci.CartId == cartId, new[] { "Product" });
                var result = items.Select(ci => new CartItemDto
                {
                    CartItemId = ci.CartItemId,
                    CartId = ci.CartId,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.ProductName,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    LineTotal = ci.LineTotal
                }).ToList();
                _logger.LogInformation($"Fetched {result.Count} cart items for CartId {cartId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching cart items for CartId {cartId}", ex);
                throw;
            }
        }

        public async Task<CartItemDto> AddCartItemAsync(int cartId, AddCartItemDto dto)
        {
            try
            {
                _logger.LogInformation($"Adding product {dto.ProductId} to cart {cartId}");

                var product = await _productRepository.QueryBy(p => p.ProductId == dto.ProductId).Result.FirstOrDefaultAsync();
                if (product == null)
                {
                    _logger.LogWarning($"Product {dto.ProductId} not found");
                    throw new Exception("Sản phẩm không tồn tại");
                }

                var existingItem = await _cartItemRepo.GetCartItemByCartAndProductAsync(cartId, dto.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += dto.Quantity;
                    await _cartItemGenericRepo.Update(existingItem);
                    _logger.LogInformation($"Updated quantity for existing cart item {existingItem.CartItemId}");
                }
                else
                {
                    existingItem = new CartItem
                    {
                        CartId = cartId,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity,
                        UnitPrice = product.SellingPrice ?? 0,
                        LineTotal = dto.Quantity * (product.SellingPrice ?? 0) 
                    };
                    await _cartItemGenericRepo.Create(existingItem);
                    UpdateCartQuantityCache(cartId, dto.Quantity);
                    _logger.LogInformation($"Created new cart item {existingItem.CartItemId}");
                }

                // 🔥 FIX: Load lại product info để đảm bảo có đầy đủ thông tin
                var updatedProduct = await _productRepository.QueryBy(p => p.ProductId == existingItem.ProductId).Result.FirstOrDefaultAsync();

                return new CartItemDto
                {
                    CartItemId = existingItem.CartItemId,
                    CartId = existingItem.CartId,
                    ProductId = existingItem.ProductId,
                    ProductName = updatedProduct?.ProductName ?? product.ProductName,
                    Quantity = existingItem.Quantity,
                    UnitPrice = existingItem.UnitPrice,
                    LineTotal = existingItem.Quantity * existingItem.UnitPrice // ✅ Tính LineTotal
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding product {dto.ProductId} to cart {cartId}", ex);
                throw;
            }
        }

        public async Task<CartItemDto> UpdateCartItemAsync(UpdateCartItemDto dto)
        {
            try
            {
                _logger.LogInformation($"Updating cart item {dto.CartItemId}");

                var item = await _cartItemGenericRepo.QueryBy(ci => ci.CartItemId == dto.CartItemId)
                    .Result.Include(ci => ci.Product).FirstOrDefaultAsync();

                if (item == null)
                {
                    _logger.LogWarning($"Cart item {dto.CartItemId} not found");
                    return null; // ✅ Thay đổi return type từ bool sang CartItemDto
                }

                item.Quantity = dto.Quantity;
                item.LineTotal = item.Quantity * item.UnitPrice; 
                await _cartItemGenericRepo.Update(item);
                _logger.LogInformation($"Updated cart item {dto.CartItemId} quantity to {dto.Quantity}");

                // ✅ Return CartItemDto với LineTotal đã cập nhật
                return new CartItemDto
                {
                    CartItemId = item.CartItemId,
                    CartId = item.CartId,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.Quantity * item.UnitPrice
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating cart item {dto.CartItemId}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteCartItemAsync(int cartItemId)
        {
            try
            {
                _logger.LogInformation($"Deleting cart item {cartItemId}");

                var item = await _cartItemGenericRepo.QueryBy(ci => ci.CartItemId == cartItemId).Result.FirstOrDefaultAsync();
                if (item == null)
                {
                    _logger.LogWarning($"Cart item {cartItemId} not found");
                    return false;
                }

                await _cartItemGenericRepo.Delete(item);
                _logger.LogInformation($"Deleted cart item {cartItemId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting cart item {cartItemId}", ex);
                throw;
            }
        }
        private void UpdateCartQuantityCache(int cartId, int quantityChange)
        {
            var key = $"cart_quantity_{cartId}";

            int currentQuantity = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return 0;
            });

            _cache.Set(key, currentQuantity + quantityChange);
        }

    }
}
