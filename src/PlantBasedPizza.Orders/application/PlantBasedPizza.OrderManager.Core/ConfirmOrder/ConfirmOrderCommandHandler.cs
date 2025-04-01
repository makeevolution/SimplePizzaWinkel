using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.ConfirmOrder;

public class ConfirmOrderCommandHandler(IOrderRepository orderRepository)
{
    public async Task<OrderDto?> Handle(ConfirmOrderCommand command)
    {
        try
        {
            var order = await orderRepository.Retrieve(command.OrderIdentifier);
            if (order is null) return null;
            
            // Update both the order state as well as register the orderConfirmed event (to be published by background worker)
            order.Confirm(command.PaymentAmount);

            await orderRepository.Update(order);

            return new OrderDto(order);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}