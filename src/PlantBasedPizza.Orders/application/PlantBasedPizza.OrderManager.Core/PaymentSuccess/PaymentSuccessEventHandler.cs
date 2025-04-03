using Grpc.Core.Logging;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.PaymentSuccess;

public class PaymentSuccessEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserNotificationService _notificationService;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IFeatures _features;
    private readonly ILogger<PaymentSuccessEventHandler> _logger;

    public PaymentSuccessEventHandler(IOrderRepository orderRepository, IUserNotificationService notificationService,
        IWorkflowEngine workflowEngine, IFeatures features, ILogger<PaymentSuccessEventHandler> logger)
    {
        _orderRepository = orderRepository;
        _notificationService = notificationService;
        _workflowEngine = workflowEngine;
        _features = features;
        _logger = logger;
    }

    [Channel("payments.paymentSuccessful.v1")]
    [PublishOperation(typeof(PaymentSuccessfulEventV1), OperationId = nameof(PaymentSuccessfulEventV1))]
    public async Task Handle(PaymentSuccessfulEventV1 evt)
    {
        _logger.LogInformation($"Handling payment successful event for order: {evt.OrderIdentifier}");
        var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

        if (_features.UseOrchestrator())
        {
            await _workflowEngine.ConfirmPayment(order.OrderIdentifier, evt.Amount);
        }
        else
        {
            order.Confirm(evt.Amount);

            await _orderRepository.Update(order);
        }

        await _notificationService.NotifyPaymentSuccess(order.CustomerIdentifier, evt.OrderIdentifier);
    }
}