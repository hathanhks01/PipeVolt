using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.Services
{
    public interface ICartService
    {
        Task<CartDto> AddCartAsync(CreateCartDto dto);
        Task<CartDto> AddItemToCartAsync(int customerId, AddCartItemDto itemDto);
        Task<int> CheckoutAsync(int customerId);
        Task<bool> DeleteCartAsync(int id);
        Task<CartDto> GetCartAsync(int customerId);
        Task<CartDto> RemoveCartItemAsync(int customerId, int cartItemId);
        Task<CartDto> UpdateCartAsync(int id, UpdateCartDto dto);
        Task<CartDto> UpdateCartItemAsync(int customerId, UpdateCartItemDto itemDto);
    }
}