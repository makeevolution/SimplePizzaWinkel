using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.PaymentSuccess;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.PaymentFailed;

public class PaymentFailedEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserNotificationService _notificationService;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IFeatures _features;
    private readonly ILogger<PaymentFailedEventHandler> _logger;

    public PaymentFailedEventHandler(IOrderRepository orderRepository, IUserNotificationService notificationService,
        IWorkflowEngine workflowEngine, IFeatures features, ILogger<PaymentFailedEventHandler> logger)
    {
        _orderRepository = orderRepository;
        _notificationService = notificationService;
        _workflowEngine = workflowEngine;
        _features = features;
        _logger = logger;
    }

    [Channel("payments.paymentFailed.v1")]
    [PublishOperation(typeof(PaymentFailedEventV1), OperationId = nameof(PaymentFailedEventV1))]
    public async Task Handle(PaymentFailedEventV1 evt)
    {
        // TODO: add support for orchestrator WOW
        _logger.LogInformation($"Handling payment failed event for order: {evt.OrderIdentifier}");
        var order = await _orderRepository.Retrieve(evt.OrderIdentifier);
        
        order.AddHistory("Payment failed!");
        order.CancelOrder(checkTimeout: true);
        await _orderRepository.Update(order);

        await _notificationService.NotifyPaymentFailed(order.CustomerIdentifier, evt.OrderIdentifier);
    }
}