using System.Diagnostics;
using System.Text.Json;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.Payments.Core.Adapters.Repositories;
using PlantBasedPizza.Payments.Core.Entities;
using PlantBasedPizza.Payments.Core.RefundPayment;
using PlantBasedPizza.Payments.Core.TakePayment;

namespace PlantBasedPizza.Payments;

public static class EventHandlers
{
    public static WebApplication AddEventHandlers(this WebApplication app)
    {
        app.MapPost("/take-payment",
            [Topic("payments", "payments.takepayment.v1", DeadLetterTopic = "payments.failedMessages")]
            async ([FromServices] TakePaymentCommandHandler handler, IDistributedCache cache, HttpContext ctx,
                TakePaymentCommand command) =>
            {
                try
                {
                    // Add OTEL tags to the event received; the event data is in the header of the request
                    var cloudEventId = ctx.ExtractEventId();
                    
                    // Use the cloudEvent ID to implement idempotency
                    var cachedEvent = await cache.GetStringAsync($"events_{cloudEventId}");
                    if (cachedEvent != null)
                    {
                        Activity.Current?.AddTag("events.idempotent", "true");
                        return Results.Ok();
                    }
                    
                    // The event is new; handle the event appropriately
                    var result = await handler.Handle(command);

                    if (!result)
                    {
                        return Results.InternalServerError();
                    }

                    // Cache the handled event
                    await cache.SetStringAsync($"events_{cloudEventId}", "processed", new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    });
                    Activity.Current?.AddEvent(new ActivityEvent(""));
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Activity.Current?.AddException(ex);
                    
                    return Results.InternalServerError();
                }
            });
        
        app.MapPost("/refund-payment",
            [Topic("payments", "payments.refundpayment.v1", DeadLetterTopic = "payments.failedMessages")]
            async ([FromServices] RefundPaymentCommandHandler handler, IDistributedCache cache, HttpContext ctx,
                RefundPaymentCommand command) =>
            {
                try
                {
                    var cloudEventId = ctx.ExtractEventId();
                
                    var cachedEvent = await cache.GetStringAsync($"events_{cloudEventId}");

                    if (cachedEvent != null)
                    {
                        Activity.Current?.AddTag("events.idempotent", "true");
                        return Results.Ok();
                    }
                
                    var result = await handler.Handle(command);

                    if (!result)
                    {
                        return Results.InternalServerError();
                    }

                    await cache.SetStringAsync($"events_{cloudEventId}", "processed", new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    });

                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Activity.Current?.AddException(ex);
                    
                    return Results.InternalServerError();
                }
            });

        return app;
    }
    
    [Topic("public", "payments.failedMessages")]
    public static async Task<IResult> HandleDeadLetterMessage(
        [FromServices] IDeadLetterRepository deadLetterRepository,
        HttpContext httpContext,
        object data)
    {
        var eventData = httpContext.ExtractEventData();

        await deadLetterRepository.StoreAsync(new DeadLetterMessage
        {
            EventId = eventData.EventId,
            EventType = eventData.EventType,
            EventData = JsonSerializer.Serialize(data),
            TraceParent = eventData.TraceParent
        });

        return Results.Ok();
    }
}