using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.Services
{
    public interface IOrderDetailService
    {
        Task<OrderDetailDto> AddOrderDetailAsync(CreateOrderDetailDto dto);
        Task<bool> DeleteOrderDetailAsync(int id);
        Task<List<OrderDetailDto>> GetAllOrderDetailsAsync();
        Task<OrderDetailDto> GetOrderDetailByIdAsync(int id);
        Task<OrderDetailDto> UpdateOrderDetailAsync(int id, UpdateOrderDetailDto dto);
    }
}