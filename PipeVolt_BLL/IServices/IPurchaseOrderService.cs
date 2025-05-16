using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.IServices
{
    public interface IPurchaseOrderService
    {
        Task<PurchaseOrderDto> AddPurchaseOrderAsync(CreatePurchaseOrderDto dto);
        Task<bool> DeletePurchaseOrderAsync(int id);
        Task<List<PurchaseOrderDto>> GetAllPurchaseOrdersAsync();
        Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(int id);
        Task<PurchaseOrderDto> UpdatePurchaseOrderAsync(int id, UpdatePurchaseOrderDto dto);
    }
}