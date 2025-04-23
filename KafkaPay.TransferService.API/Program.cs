using KafkaPay.Shared.Application;
using KafkaPay.Shared.Infrastructure;
using KafkaPay.TransferService.Application;
using KafkaPay.TransferService.Infrastructure;
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


Log.Logger = LoggingConfig.Create("TransactionService")
    .ReadFrom.Configuration(builder.Configuration) // Optional: Load appsettings
    .MinimumLevel.Error()
    .CreateLogger();

builder.Host.UseSerilog();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
