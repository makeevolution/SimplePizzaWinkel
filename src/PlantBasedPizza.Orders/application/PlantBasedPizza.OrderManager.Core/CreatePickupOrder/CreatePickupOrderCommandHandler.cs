using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.CreatePickupOrder;

public class CreatePickupOrderCommandHandler(IOrderRepository orderRepository, ILogger<CreatePickupOrderCommandHandler> logger)
{
    public async Task<OrderDto?> Handle(CreatePickupOrderCommand request)
    {
        logger.LogInformation("Handling CreatePickupOrderCommand");
        var order = Order.Create(CreatePickupOrderCommand.OrderType, request.CustomerIdentifier);

        await orderRepository.Add(order);

        return new OrderDto(order);
    }
}