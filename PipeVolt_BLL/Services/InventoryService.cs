using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IGenericRepository<Inventory> _repo;
        private readonly IGenericRepository<Warehouse> _warehouseRepo;
        private readonly IGenericRepository<PurchaseOrder> _PurchaseOrderRepo;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public InventoryService(
            IGenericRepository<Inventory> repo,
            IGenericRepository<Warehouse> warehouseRepo,
            IGenericRepository<PurchaseOrder> PurchaseOrderRepo,
            ILoggerService logger,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _warehouseRepo = warehouseRepo ?? throw new ArgumentNullException(nameof(warehouseRepo));
            _PurchaseOrderRepo = PurchaseOrderRepo ?? throw new ArgumentNullException(nameof(PurchaseOrderRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<InventoryDto>> GetAllInventoriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all inventories");
                var inventories = await _repo.GetAll();
                var result = _mapper.Map<List<InventoryDto>>(inventories);
                _logger.LogInformation($"Fetched {result.Count} inventories");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching all inventories", ex);
                throw;
            }
        }

        /// <summary>
        /// Nhập kho từ đơn hàng mua.
        /// Nếu đã có bản ghi tồn kho cho (warehouseId, productId) thì CỘNG DỒN số lượng.
        /// Nếu chưa có thì tạo mới.
        /// </summary>
        public async Task<bool> ReceiveFromPurchaseOrderAsync(string warehouseCode, string purchaseOrderCode)
        {
            // 1. Tìm warehouse
            var warehouseQuery = await _warehouseRepo.QueryBy(w => w.WarehouseCode == warehouseCode);
            var warehouse = await warehouseQuery.FirstOrDefaultAsync();
            if (warehouse == null)
                throw new KeyNotFoundException($"Warehouse '{warehouseCode}' not found.");

            // 2. Lấy PurchaseOrder và các chi tiết
            var poQuery = await _PurchaseOrderRepo.QueryBy(x => x.PurchaseOrderCode == purchaseOrderCode);
            var po = await poQuery
                .Include(x => x.PurchaseOrderDetails)
                .FirstOrDefaultAsync();

            if (po == null)
                throw new KeyNotFoundException($"Purchase order '{purchaseOrderCode}' not found.");

            // 3. Với từng dòng chi tiết: cộng dồn hoặc tạo mới
            foreach (var detail in po.PurchaseOrderDetails)
            {
                var existingQuery = await _repo.QueryBy(
                    i => i.WarehouseId == warehouse.WarehouseId && i.ProductId == detail.ProductId);
                var existing = await existingQuery.FirstOrDefaultAsync();

                if (existing != null)
                {
                    // Cộng dồn vào bản ghi hiện có
                    existing.Quantity += detail.Quantity ?? 0;
                    existing.UpdatedAt = DateTime.Now;
                    // Ghi nhận PO cuối cùng nhập cho dòng này (tuỳ nghiệp vụ có thể bỏ)
                    existing.PurchaseOrderId = po.PurchaseOrderId;
                    await _repo.Update(existing);

                    _logger.LogInformation(
                        $"Updated inventory: warehouse={warehouse.WarehouseId}, " +
                        $"product={detail.ProductId}, added={detail.Quantity}, " +
                        $"new total={existing.Quantity}");
                }
                else
                {
                    // Chưa có bản ghi → tạo mới
                    var inventory = new Inventory
                    {
                        WarehouseId = warehouse.WarehouseId,
                        ProductId = detail.ProductId,
                        PurchaseOrderId = po.PurchaseOrderId,
                        Quantity = detail.Quantity ?? 0,
                        UpdatedAt = DateTime.Now
                    };
                    await _repo.Create(inventory);

                    _logger.LogInformation(
                        $"Created inventory: warehouse={warehouse.WarehouseId}, " +
                        $"product={detail.ProductId}, quantity={detail.Quantity}");
                }
            }

            return true;
        }

        public async Task<List<InventoryProductDto>> GetInventoriesByWarehouseCodeAsync(string warehouseCode)
        {
            if (string.IsNullOrWhiteSpace(warehouseCode))
            {
                _logger.LogWarning("Mã kho hàng không được để trống.");
                throw new ArgumentException("Mã kho hàng không được để trống.");
            }

            _logger.LogInformation($"Đang lấy danh sách sản phẩm trong tồn kho cho kho {warehouseCode}");

            try
            {
                var warehouseQuery = await _warehouseRepo.QueryBy(w => w.WarehouseCode == warehouseCode);
                var warehouse = await warehouseQuery.FirstOrDefaultAsync();

                if (warehouse == null)
                {
                    _logger.LogWarning($"Không tìm thấy kho hàng với mã {warehouseCode}");
                    throw new KeyNotFoundException($"Kho hàng với mã {warehouseCode} không tồn tại.");
                }

                var inventoryQuery = await _repo.QueryBy(i => i.WarehouseId == warehouse.WarehouseId);
                var inventoryList = await inventoryQuery
                    .Include(i => i.Product)
                        .ThenInclude(p => p.Category)
                    .Include(i => i.Product)
                        .ThenInclude(p => p.Brand)
                    .ToListAsync();

                // Gộp theo ProductId (cộng tất cả bản ghi tồn kho của cùng sản phẩm trong kho)
                var result = inventoryList
                    .GroupBy(i => i.ProductId)
                    .Select(g => new InventoryProductDto
                    {
                        Product = _mapper.Map<ProductDto>(g.First().Product),
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                _logger.LogInformation($"Đã lấy {result.Count} sản phẩm trong tồn kho của kho {warehouseCode}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy danh sách sản phẩm trong tồn kho cho kho {warehouseCode}", ex);
                throw;
            }
        }

        public async Task<InventoryDto> GetInventoryByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching inventory with ID {id}");
                var inventories = await _repo.QueryBy(x => x.InventoryId == id);
                var entity = inventories.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Inventory with ID {id} not found");
                    throw new KeyNotFoundException("Inventory not found.");
                }
                var result = _mapper.Map<InventoryDto>(entity);
                _logger.LogInformation($"Fetched inventory with ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching inventory with ID {id}", ex);
                throw;
            }
        }

        public async Task<List<InventoryDto>> GetInventoriesBywarehouseIdAsync(int warehouseId)
        {
            try
            {
                _logger.LogInformation($"Fetching inventories for warehouse ID {warehouseId}");
                var inventories = await _repo.QueryBy(x => x.WarehouseId == warehouseId);
                var result = _mapper.Map<List<InventoryDto>>(await inventories.ToListAsync());
                _logger.LogInformation($"Fetched {result.Count} inventories for warehouse ID {warehouseId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching inventories for warehouse ID {warehouseId}", ex);
                throw;
            }
        }

        public async Task<InventoryDto> AddInventoryAsync(CreateInventoryDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("Invalid inventory data: DTO is null");
                throw new ArgumentException("Invalid inventory data");
            }

            try
            {
                _logger.LogInformation("Adding new inventory");
                var entity = _mapper.Map<Inventory>(dto);
                var created = await _repo.Create(entity);
                var result = _mapper.Map<InventoryDto>(created);
                _logger.LogInformation($"Added inventory with ID {result.InventoryId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding inventory", ex);
                throw;
            }
        }

        public async Task<InventoryDto> UpdateInventoryAsync(int id, UpdateInventoryDto dto)
        {
            if (dto == null || dto.InventoryId != id)
            {
                _logger.LogWarning("Invalid inventory update: ID mismatch or DTO is null");
                throw new ArgumentException("Invalid inventory update request");
            }

            try
            {
                _logger.LogInformation($"Updating inventory with ID {id}");
                var inventories = await _repo.QueryBy(x => x.InventoryId == id);
                var entity = inventories.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Inventory with ID {id} not found");
                    throw new KeyNotFoundException("Inventory not found.");
                }

                _mapper.Map(dto, entity);
                await _repo.Update(entity);
                var result = _mapper.Map<InventoryDto>(entity);
                _logger.LogInformation($"Updated inventory with ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating inventory with ID {id}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting inventory with ID {id}");
                var inventories = await _repo.QueryBy(x => x.InventoryId == id);
                var entity = inventories.FirstOrDefault();
                if (entity == null)
                {
                    _logger.LogWarning($"Inventory with ID {id} not found");
                    throw new KeyNotFoundException("Inventory not found.");
                }

                await _repo.Delete(entity);
                _logger.LogInformation($"Deleted inventory with ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting inventory with ID {id}", ex);
                throw;
            }
        }
    }
}
