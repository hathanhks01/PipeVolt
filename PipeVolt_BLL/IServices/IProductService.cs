using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.Services
{
    public interface IProductService
    {
        Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
        Task<bool> DeleteProductAsync(int productId);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetPopularProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int productId);
        Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto productDto);
        Task<List<ProductDto>> GetProductsByCategoryIdAsync(int categoryId);
        Task<List<ProductDto>> SearchTemp(string keyword);
    }
}