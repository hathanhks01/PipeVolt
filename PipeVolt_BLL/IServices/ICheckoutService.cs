using PipeVolt_DAL.DTOS;

namespace PipeVolt_BLL.IServices
{
    public interface ICheckoutService
    {
        Task<int> CheckoutAsync(int customerId, int paymentMethodId);
        Task<int> CreateOrderAndCheckoutAsync(int customerId, int paymentMethodId, List<int> cartItemIds);
        Task<int> PosCheckoutAsync(List<PosItem> items, int paymentMethodId, PosCustomerInfo customerInfo = null, int? cashierId = null, decimal discountPercent = 0);

    }
}