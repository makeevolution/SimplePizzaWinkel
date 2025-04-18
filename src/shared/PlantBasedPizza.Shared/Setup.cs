using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PlantBasedPizza.Shared.Logging;
using Saunter;
using Saunter.AsyncApiSchema.v2;

namespace PlantBasedPizza.Shared
{
    public static class Setup
    {
        private const string OTEL_DEFAULT_GRPC_ENDPOINT = "http://seq:5341/ingest/otlp/v1/traces";
        
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services,
            IConfiguration configuration, string applicationName, string[]? additionalSources = null)
        {
            services.AddLogging();
            
            // The AddOpenTelemetry below is supplied by OpenTelemetry.Extensions.Hosting package
            var otel = services.AddOpenTelemetry();
            otel.ConfigureResource(resource => resource
                .AddDefaultOtelTags(configuration)
                .AddService(serviceName: applicationName));
            
            otel.WithTracing(tracing =>  // Configure tracing
            {
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = new Func<HttpContext, bool>((httpContext) =>
                    {
                        try
                        {
                            if (httpContext.Request.Path.Value.Contains("/notifications"))
                            {
                                return false;
                            }

                            return true;
                        }
                        catch
                        {
                            return true;
                        }
                    });
                });
                tracing.AddGrpcClientInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddSource(applicationName);

                if (additionalSources != null)
                {
                    foreach (var source in additionalSources)
                    {
                        tracing.AddSource(source);
                    }
                }

                tracing.AddOtlpExporter(); // Add the default localhost:4317
                tracing.AddOtlpExporter(otlpOptions =>
                {
                    // Export OTEL also to Seq if the environment variable is set
                    otlpOptions.Endpoint = new Uri(configuration["OTEL_EXPORTER_SEQ"] ?? OTEL_DEFAULT_GRPC_ENDPOINT);
                    otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            });
            
            services.AddHttpContextAccessor();

            services.AddCors(options =>
            {
                options.AddPolicy(name: CorsSettings.ALLOW_ALL_POLICY_NAME,
                    policy  =>
                    {
                        policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            return services;
        }

        public static IServiceCollection AddAsyncApiDocs(this IServiceCollection services, IConfiguration configuration, IList<Type> eventPublisherTypes, string serviceName)
        {
            var generateAsyncApi = configuration["Messaging:UseAsyncApi"] == "Y";

            if (generateAsyncApi)
            {
                services.AddAsyncApiSchemaGeneration(options =>
                {
                    options.AssemblyMarkerTypes = eventPublisherTypes;

                    options.AsyncApi = new AsyncApiDocument
                    {
                        Info = new Info(serviceName, "1.0.0"),
                    };
                });   
            }

            return services;
        }

        public static WebApplication UseAsyncApi(this WebApplication app)
        {
            var generateAsyncApi = app.Configuration["Messaging:UseAsyncApi"] == "Y";
            
            app.UseEndpoints(endpoints =>
            {
                if (generateAsyncApi)
                {
                    endpoints.MapAsyncApiDocuments();
                    endpoints.MapAsyncApiUi();    
                }
            });
            
            return app;
        }
        
        public static IApplicationBuilder UseSharedMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseUserExtractionMiddleware();
        }
    }
}