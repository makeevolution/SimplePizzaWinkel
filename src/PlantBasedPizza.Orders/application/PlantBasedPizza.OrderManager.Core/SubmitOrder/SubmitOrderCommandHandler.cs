using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.SubmitOrder;

public class SubmitOrderCommandHandler(IOrderRepository orderRepository)
{
    public async Task<OrderDto> Handle(SubmitOrderCommand command)
    {
        var order = await orderRepository.Retrieve(command.OrderIdentifier);

        if (order.CustomerIdentifier != command.CustomerIdentifier)
            throw new OrderNotFoundException(command.OrderIdentifier);
        
        // Update both the order state as well as register the orderSubmitted event (to be published by background worker)
        order.SubmitOrder();
        
        // Calling this update method will auto update (in one transaction) two things:
        // - The changes to the order state (e.g. its submittedOn property)
        // - The outbox database with the event to be published, which will be processed by the background outbox worker
        // Directly publishing after changing order state risks that if the app/event bus goes down in-between state
        // update and event publish, the state would have been updated but other systems do not know about this.
        // Doing outbox pattern, like what we do here, eliminates this possibility, since the state-update and "publishing"
        // is done in the same transaction/step. 
        await orderRepository.Update(order);
        
        return new OrderDto(order);
    }
}