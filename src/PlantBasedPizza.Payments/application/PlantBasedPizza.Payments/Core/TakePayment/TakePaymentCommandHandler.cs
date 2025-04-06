using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Payments.Core.PublicEvents;
using Saunter.Attributes;

namespace PlantBasedPizza.Payments.Core.TakePayment;

public class TakePaymentCommandHandler(ILogger<TakePaymentCommandHandler> logger, IPaymentEventPublisher eventPublisher, IDistributedCache cache)
{
    [Channel("payments.takepayment.v1")]
    [PublishOperation(typeof(TakePaymentCommand), OperationId = nameof(TakePaymentCommand))]
    public async Task<bool> Handle(TakePaymentCommand command)
    {
        var hasOrderBeenProcessed = await cache.GetStringAsync(command.OrderIdentifier);

        if ((hasOrderBeenProcessed ?? "").Equals("processed"))
        {
            Activity.Current?.AddTag("payment-processed", "true");
            return true;
        }

        try
        {
            ///////// Simulate contacting bank and charging it by a simple delay
            var randomSecondDelay = RandomNumberGenerator.GetInt32(1500, 2000);

            await Task.Delay(TimeSpan.FromMilliseconds(randomSecondDelay));
            
            _simulatePaymentFailure(command);

            var successEvent = new PaymentSuccessfulEventV1()
            {
                OrderIdentifier = command.OrderIdentifier,
                Amount = Convert.ToDecimal(command.PaymentAmount)
            };
            
            await eventPublisher.PublishPaymentSuccessfulEventV1(successEvent);
            
            await cache.SetStringAsync(command.OrderIdentifier, "processed");

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failure processing payment for order {OrderIdentifier}", command.OrderIdentifier);
            Activity.Current?.AddException(ex);
            
            await eventPublisher.PublishPaymentFailedEventV1(new PaymentFailedEventV1()
            {
                OrderIdentifier = command.OrderIdentifier
            });

            return false;
        }
    }

    private void _simulatePaymentFailure(TakePaymentCommand command)
    {
        //  SImulate a failure
        var failPayment = Environment.GetEnvironmentVariable("FAIL_PAYMENT");
        if (failPayment == "true")
        {
            var fail_message = $"Simulate failure processing payment for order {command.OrderIdentifier}";
            logger.LogError(fail_message);
            throw new Exception(fail_message);
        }
    }
}