using Grpc.Core;
using Grpc.Net.Client.Configuration;
using MongoDB.Driver;
using PlantBasedPizza.Orders.Internal;
using PlantBasedPizza.Payments;
using PlantBasedPizza.Payments.Core.Adapters.Repositories;
using PlantBasedPizza.Payments.Core.PublicEvents;
using PlantBasedPizza.Payments.Core.RefundPayment;
using PlantBasedPizza.Payments.Core.Services;
using PlantBasedPizza.Payments.Core.TakePayment;
using PlantBasedPizza.Payments.Infrastructure.Repositories;
using PlantBasedPizza.Payments.Infrastructure.Services;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Caching;
using PlantBasedPizza.Shared.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);
builder
    .Configuration
    .AddEnvironmentVariables();

var logger = Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();
builder.AddLoggerConfigs();

var appLogger = new SerilogLoggerFactory(logger)
    .CreateLogger<Program>();

// Add services to the container.
builder.Services.AddGrpc();

builder.Services
    .AddSharedInfrastructure(builder.Configuration, "PaymentApi")
    .AddAsyncApiDocs(builder.Configuration, [typeof(PaymentEventPublisher), typeof(RefundPaymentCommandHandler), typeof(TakePaymentCommandHandler)], "PaymentApi")
    .AddCaching(builder.Configuration);  // caching to cache already processed events (for idempotency)

builder.Services.AddSingleton<TakePaymentCommandHandler>();
builder.Services.AddSingleton<RefundPaymentCommandHandler>();
builder.Services.AddSingleton<IPaymentEventPublisher, PaymentEventPublisher>();
builder.Services.AddSingleton<IDeadLetterRepository, DeadLetterRepository>();
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddSingleton(new MongoClient(builder.Configuration["DatabaseConnection"]));
builder.Services.AddDaprClient();

// Add default gRPC retries
var defaultMethodConfig = new MethodConfig
{
    Names = { MethodName.Default },
    RetryPolicy = new RetryPolicy
    {
        MaxAttempts = 5,
        InitialBackoff = TimeSpan.FromSeconds(1),
        MaxBackoff = TimeSpan.FromSeconds(5),
        BackoffMultiplier = 1.5,
        RetryableStatusCodes = { StatusCode.Unavailable }
    }
};

builder.Services.AddGrpcClient<Orders.OrdersClient>(o =>
    {
        o.Address = new Uri(builder.Configuration["Services:OrdersInternal"]);
    })
    .ConfigureChannel((provider, channel) =>
    {
        channel.ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } };
    });

var app = builder.Build();

app.MapGet("/payments/health", () => "Healthy");
app.MapPost("/event-dead-letter", EventHandlers.HandleDeadLetterMessage);

app.UseRouting();

app.MapSubscribeHandler();
app.UseCloudEvents();
app.AddEventHandlers();

app.UseAsyncApi();

appLogger.LogInformation("Running!");

await app.RunAsync();