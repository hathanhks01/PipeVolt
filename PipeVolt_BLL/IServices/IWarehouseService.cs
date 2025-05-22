using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.IServices
{
    public interface IWarehouseService
    {
        Task<WarehouseDto> AddWarehouseAsync(CreateWarehouseDto dto);
        Task<bool> DeleteWarehouseAsync(int id);
        Task<List<WarehouseDto>> GetAllWarehousesAsync();
        Task<WarehouseDto> GetWarehouseByIdAsync(int id);
        Task<WarehouseDto> UpdateWarehouseAsync(int id, UpdateWarehouseDto dto);
    }
}