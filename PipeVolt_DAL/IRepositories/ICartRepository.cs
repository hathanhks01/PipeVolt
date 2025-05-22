using PipeVolt_DAL.Models;

namespace PipeVolt_DAL.IRepositories
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByCustomerIdAsync(int customerId);
    }
}