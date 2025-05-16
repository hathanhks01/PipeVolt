using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.IServices
{
    public interface ISupplierService
    {
        Task<SupplierDto> AddSupplierAsync(CreateSupplierDto dto);
        Task<bool> DeleteSupplierAsync(int id);
        Task<List<SupplierDto>> GetAllSuppliersAsync();
        Task<SupplierDto> GetSupplierByIdAsync(int id);
        Task<SupplierDto> UpdateSupplierAsync(int id, UpdateSupplierDto dto);
    }
}