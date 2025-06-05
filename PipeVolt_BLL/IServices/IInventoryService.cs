using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.IServices
{
    public interface IInventoryService
    {
        Task<InventoryDto> AddInventoryAsync(CreateInventoryDto dto);
        Task<List<InventoryProductDto>> GetInventoriesByWarehouseCodeAsync(string warehouseCode);
        Task<bool> DeleteInventoryAsync(int id);
        Task<List<InventoryDto>> GetAllInventoriesAsync();
        Task<InventoryDto> GetInventoryByIdAsync(int id);
        Task<InventoryDto> UpdateInventoryAsync(int id, UpdateInventoryDto dto);
        Task<bool> ReceiveFromPurchaseOrderAsync(string warehouseCode, int purchaseOrderId);
    }
}