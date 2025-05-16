using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.IServices
{
    public interface ISupplyService
    {
        Task<SupplyDto> AddSupplyAsync(CreateSupplyDto dto);
        Task<bool> DeleteSupplyAsync(int id);
        Task<List<SupplyDto>> GetAllSuppliesAsync();
        Task<SupplyDto> GetSupplyByIdAsync(int id);
        Task<SupplyDto> UpdateSupplyAsync(int id, UpdateSupplyDto dto);
    }
}