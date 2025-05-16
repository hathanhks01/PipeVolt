using PipeVolt_DAL.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.IServices
{
    public interface IProductCategoryService
    {
        Task<List<ProductCategoryDto>> GetAllAsync();
        Task<ProductCategoryDto> GetByIdAsync(int id);
        Task<ProductCategoryDto> CreateAsync(CreateProductCategoryDto dto);
        Task<ProductCategoryDto> UpdateAsync(int id, UpdateProductCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
