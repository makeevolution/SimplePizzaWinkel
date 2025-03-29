using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace PlantBasedPizza.Shared.Logging;

public static class LoggerConfigs
{
    public static WebApplicationBuilder AddLoggerConfigs(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((_, config) =>
        {
            config.MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter()).WriteTo.Seq("http://seq:5341");
        });

        return builder;
    }
}