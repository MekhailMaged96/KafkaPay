using KafkaPay.Shared.Application;
using KafkaPay.Shared.Infrastructure;
using KafkaPay.TransferService.Application;
using KafkaPay.TransferService.Infrastructure;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddTransferApplicationServices();
builder.AddTransferInfrastructureServices();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();


builder.Host.UseSerilog((context, loggerconfig) =>
{
    loggerconfig.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                {
                    resource.AddService("TransferService.API");
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
