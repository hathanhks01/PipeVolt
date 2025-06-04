namespace PipeVolt_BLL.IServices
{
    public interface ICheckoutService
    {
        Task<int> CheckoutAsync(int customerId, int paymentMethodId);
        Task<int> CreateOrderAndCheckoutAsync(int customerId, int paymentMethodId, List<int> cartItemIds);
    }
}