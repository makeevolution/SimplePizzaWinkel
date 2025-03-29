using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.AddItemToOrder;

public class AddItemToOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRecipeService _recipeService;
    //private readonly ILogger _logger;

    public AddItemToOrderHandler(IOrderRepository orderRepository, 
        IRecipeService recipeService)
        //ILogger logger)
    {
        _orderRepository = orderRepository;
        _recipeService = recipeService;
        //_logger = logger;
    }
    
    public async Task<Order?> Handle(AddItemToOrderCommand command)
    {
        try
        {
            //_logger.LogInformation($"adding item to order {command.OrderIdentifier}");
            var recipe = await _recipeService.GetRecipe(command.RecipeIdentifier);
            
            var order = await _orderRepository.Retrieve(command.OrderIdentifier);

            if (order.CustomerIdentifier != command.CustomerIdentifier)
            {
                throw new OrderNotFoundException(command.OrderIdentifier);
            } 

            order.AddOrderItem(command.RecipeIdentifier, recipe.ItemName, command.Quantity, recipe.Price);

            await _orderRepository.Update(order);

            return order;
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}