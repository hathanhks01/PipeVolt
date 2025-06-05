using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.IServices
{
    public interface ISalesOrderService
    {
        Task<SalesOrderDto> AddSalesOrderAsync(CreateSalesOrderDto dto);
        Task<bool> DeleteSalesOrderAsync(int id);
        Task<List<SalesOrderDto>> GetAllSalesOrdersAsync();
        Task<SalesOrderDto> GetSalesOrderByIdAsync(int id);
        Task<SalesOrderDto> UpdateSalesOrderAsync(int id, UpdateSalesOrderDto dto);
        Task Checkout(CheckoutDto checkoutDto);
    }
}