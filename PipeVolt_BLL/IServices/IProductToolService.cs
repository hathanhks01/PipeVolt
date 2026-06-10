// PipeVolt_BLL/IServices/IProductToolService.cs
namespace PipeVolt_BLL.IServices
{
    public interface IProductToolService
    {
        Task<string> SearchProductsAsync(string keyword, int? categoryId = null);
        Task<string> GetProductDetailAsync(int productId);
        Task<string> CheckInventoryAsync(int productId);
        Task<string> GetCategoriesAsync();
        Task<string> GetProductsByPriceRangeAsync(double minPrice, double maxPrice);
        Task<string> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters);
    }
}