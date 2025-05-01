using KafkaPay.AccountingService.Application;
using KafkaPay.Shared.Application;
using KafkaPay.Shared.Infrastructure;
using Serilog;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                {
                    resource.AddService("KafkaPay.TransferService.API");
                }).WithTracing(tracing =>
                {
                    tracing.AddHttpClientInstrumentation()
                           .AddAspNetCoreInstrumentation()
                           .AddSource("Kafka.Produce")
                           .AddSource("Kafka.Consume")
                           .AddConsoleExporter();
                    tracing.AddOtlpExporter(options =>
                      {
                          options.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
                          options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                      });

                });
Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(
    new TextMapPropagator[] { new TraceContextPropagator(), new BaggagePropagator() }
));

builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddAccountApplicationServices();



builder.Host.UseSerilog((context, loggerconfig) =>
{ 
    loggerconfig.ReadFrom.Configuration(context.Configuration);
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();

app.Run();
