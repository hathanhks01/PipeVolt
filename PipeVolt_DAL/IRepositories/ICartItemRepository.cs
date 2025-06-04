using PipeVolt_DAL.Models;

namespace PipeVolt_DAL.Repositories
{
    public interface ICartItemRepository
    {
        Task DeleteRange(Func<CartItem, bool> predicate);
        Task<CartItem> GetCartItemByCartAndProductAsync(int cartId, int productId);
    }
}