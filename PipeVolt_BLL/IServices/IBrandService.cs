using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;

namespace PipeVolt_BLL.Services
{
    public interface IBrandService
    {
        Task<BrandDto> AddBrandAsync(BrandDto brand);
        Task<bool> DeleteProductAsync(int BrandId);
        Task<List<BrandDto>> GetAllBrandsAsync();
        Task<BrandDto?> GetBrandByIdAsync(int id);
        Task<BrandDto> UpdateBrandAsync(int id, BrandDto brand);
    }
}