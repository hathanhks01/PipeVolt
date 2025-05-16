using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.IServices
{
    public interface IPurchaseOrderDetailService
    {
        Task<PurchaseOrderDetailDto> AddPurchaseOrderDetailAsync(CreatePurchaseOrderDetailDto dto);
        Task<bool> DeletePurchaseOrderDetailAsync(int id);
        Task<List<PurchaseOrderDetailDto>> GetAllPurchaseOrderDetailsAsync();
        Task<PurchaseOrderDetailDto> GetPurchaseOrderDetailByIdAsync(int id);
        Task<PurchaseOrderDetailDto> UpdatePurchaseOrderDetailAsync(int id, UpdatePurchaseOrderDetailDto dto);
    }
}