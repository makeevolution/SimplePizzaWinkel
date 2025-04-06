namespace PlantBasedPizza.Payments.Core.Services;

public interface IOrderService
{
    Task<Order> GetOrderDetails(string orderIdentifier);
}