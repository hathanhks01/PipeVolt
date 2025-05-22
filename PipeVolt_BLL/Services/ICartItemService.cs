using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.Services
{
    public interface ICartItemService
    {
        Task<CartItemDto> AddCartItemAsync(int cartId, AddCartItemDto dto);
        Task<bool> DeleteCartItemAsync(int cartItemId);
        Task<IEnumerable<CartItemDto>> GetCartItemsByCartIdAsync(int cartId);
        Task<bool> UpdateCartItemAsync(UpdateCartItemDto dto);
    }
}