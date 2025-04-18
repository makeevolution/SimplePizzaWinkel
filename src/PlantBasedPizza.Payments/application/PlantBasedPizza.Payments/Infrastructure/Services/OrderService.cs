using Grpc.Core;
using PlantBasedPizza.Orders.Internal;
using PlantBasedPizza.Payments.Core.Services;

namespace PlantBasedPizza.Payments.Infrastructure.Services;

public class OrderService(Orders.Internal.Orders.OrdersClient orderClient) : IOrderService
{
    private readonly Metadata metadata = new()
    {
        { "dapr-app-id", "orders-internal" }
    };
    
    public async Task<Order> GetOrderDetails(string orderIdentifier)
    {
        var order = await orderClient.GetOrderDetailsAsync(new GetOrderDetailsRequest()
        {
            OrderIdentifier = orderIdentifier
        }, metadata);

        return new Order()
        {
            OrderIdentifier = order.OrderIdentifier,
            OrderValue = Convert.ToDecimal(order.OrderValue),
        };
    }
}