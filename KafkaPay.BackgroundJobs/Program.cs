using Hangfire;
using Hangfire.MemoryStorage;
using KafkaPay.BackgroundJobs;
using KafkaPay.BackgroundJobs.Jobs;
using KafkaPay.Shared.Application;
using KafkaPay.Shared.Infrastructure;
using KafkaPay.Shared.Infrastructure.Backgrounds.Jobs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Services.AddHostedService<Worker>();

builder.Services.AddHangfire(config =>
    config.UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseMemoryStorage()); // Replace with SqlServer, Redis in prod

builder.Services.AddHangfireServer();
builder.Services.AddScoped<ProcessOutboxMessageJob>();
builder.AddInfrastructureServices();
builder.AddApplicationServices();

builder.Services.AddSingleton<IRecurringJobRegistrar, RecurringJobsRegistrar>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                {
                    resource.AddService("BackgroundJobs");
                }).WithTracing(tracing =>
                {
                    tracing.AddHttpClientInstrumentation()
                           .AddAspNetCoreInstrumentation()
                           .AddConsoleExporter();

                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;

                    });

                });


var host = builder.Build();


using (var scope = host.Services.CreateScope())
{
    var jobRegistrar = scope.ServiceProvider.GetRequiredService<IRecurringJobRegistrar>();
    jobRegistrar.RegisterJobs();
}

host.Run();
