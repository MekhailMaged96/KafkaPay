using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot();
// Ensure this is added

builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                {
                    resource.AddService("KafkaPay.ApiGatway");
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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseOcelot().Wait();

app.Run();
