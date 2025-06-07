using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;

namespace PipeVolt_BLL.IServices
{
    public interface ICustomerService
    {
        Task<CustomerDto> AddCustomerAsync(CreateCustomerDto dto);
        Task<bool> DeleteCustomerAsync(int customerId);
        Task<List<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
        Task<CustomerDto?> GetCustomerByUserIdAsync(int userId);
    }
}