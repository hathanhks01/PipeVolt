using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.Services
{
    public interface IWarrantyService
    {
        Task<WarrantyDto> AddWarrantyAsync(CreateWarrantyDto dto);
        Task<bool> DeleteWarrantyAsync(int id);
        Task<List<WarrantyDto>> GetAllWarrantiesAsync();
        Task<WarrantyDto> GetWarrantyByIdAsync(int id);
        Task<WarrantyDto> UpdateWarrantyAsync(int id, UpdateWarrantyDto dto);
    }
}