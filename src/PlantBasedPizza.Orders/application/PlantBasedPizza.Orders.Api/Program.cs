using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PlantBasedPizza.OrderManager.Core.DriverCollectedOrder;
using PlantBasedPizza.OrderManager.Core.DriverDeliveredOrder;
using PlantBasedPizza.OrderManager.Core.OrderBaked;
using PlantBasedPizza.OrderManager.Core.OrderPreparing;
using PlantBasedPizza.OrderManager.Core.OrderPrepComplete;
using PlantBasedPizza.OrderManager.Core.OrderQualityChecked;
using PlantBasedPizza.OrderManager.Core.PaymentSuccess;
using PlantBasedPizza.OrderManager.Core.PaymentFailed;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.OrderManager.Infrastructure.HealthChecks;
using PlantBasedPizza.Orders.Api;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Authentication;
using PlantBasedPizza.Shared.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

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

builder.Services.ConfigureAuth(builder.Configuration);

var applicationName = "OrdersApi";

builder.Services.AddOrderManagerInfrastructure(builder.Configuration)
    .AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddAsyncApiDocs(builder.Configuration, 
        [typeof(OrderEventPublisher),
            typeof(DriverDeliveredOrderEventHandler),
            typeof(DriverCollectedOrderEventHandler),
            typeof(OrderBakedEventHandler),
            typeof(OrderPreparingEventHandler),
            typeof(OrderPrepCompleteEventHandler),
            typeof(OrderQualityCheckedEventHandler),
            typeof(PaymentSuccessEventHandler),
            typeof(PaymentFailedEventHandler),
        ]
        , "OrdersService");

builder.Services.AddHttpClient()
    .RemoveAll<IHttpMessageHandlerBuilderFilter>()
    .AddHealthChecks()
    .AddCheck<LoyaltyServiceHealthChecks>("LoyaltyService")
    .AddCheck<RecipeServiceHealthCheck>("RecipeService")
    .AddCheck<DeadLetterQueueChecks>("DeadLetterQueue")
    .AddMongoDb(builder.Configuration["DatabaseConnection"]);

var app = builder.Build();

app.UseCors(CorsSettings.ALLOW_ALL_POLICY_NAME)
    .UseAuthentication()
    .UseRouting()
    .UseAuthorization()
    .UseSharedMiddleware();

app.MapHealthChecks("/order/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
});


string[] AllowAllRoles = new[] { "user", "staff", "admin" };
string[] AllowStaffRoles = new[] { "admin", "staff" };

app.MapGet("/order", OrderEndpoints.GetForCustomer)
    .RequireAuthorization(options => options.RequireRole(AllowAllRoles));
app.MapGet("/order/{orderIdentifier}/detail", OrderEndpoints.Get)
    .RequireAuthorization(options => options.RequireRole(AllowAllRoles));
app.MapPost("/order/pickup", OrderEndpoints.CreatePickupOrder)
    .RequireAuthorization(options => options.RequireRole(AllowAllRoles));
app.MapPost("/order/deliver", OrderEndpoints.CreateDeliveryOrder)
    .RequireAuthorization(options => options.RequireRole(AllowAllRoles));
app.MapPost("/order/{orderIdentifier}/items", OrderEndpoints.AddItemToOrder)
    .RequireAuthorization(options => options.RequireRole(AllowAllRoles));
app.MapPost("/order/{orderIdentifier}/submit", OrderEndpoints.SubmitOrder)
    .RequireAuthorization(options => options.RequireRole(AllowAllRoles));
app.MapPost("/order/{orderIdentifier}/cancel", OrderEndpoints.CancelOrder)
    .RequireAuthorization(options => options.RequireRole(AllowAllRoles));
app.MapGet("/order/awaiting-collection", OrderEndpoints.GetAwaitingCollection)
    .RequireAuthorization(options => options.RequireRole(AllowStaffRoles));
app.MapPost("/order/collected", OrderEndpoints.OrderCollected)
    .RequireAuthorization(options => options.RequireRole(AllowStaffRoles));

app.UseAsyncApi();

appLogger.LogInformation("Running!");

await app.RunAsync();

static Task WriteHealthCheckResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var options = new JsonWriterOptions { Indented = true };

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("status", healthReport.Status.ToString());
        jsonWriter.WriteStartObject("results");

        foreach (var healthReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject(healthReportEntry.Key);
            jsonWriter.WriteString("status",
                healthReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("description",
                healthReportEntry.Value.Description);
            jsonWriter.WriteStartObject("data");

            foreach (var item in healthReportEntry.Value.Data)
            {
                jsonWriter.WritePropertyName(item.Key);

                JsonSerializer.Serialize(jsonWriter, item.Value,
                    item.Value?.GetType() ?? typeof(object));
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();
    }

    return context.Response.WriteAsync(
        Encoding.UTF8.GetString(memoryStream.ToArray()));
}