using Hangfire;
using Hangfire.MemoryStorage;
using KafkaPay.BackgroundJobs;
using KafkaPay.BackgroundJobs.Jobs;
using KafkaPay.Shared.Application;
using KafkaPay.Shared.Infrastructure;
using KafkaPay.Shared.Infrastructure.Backgrounds.Jobs;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);


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

builder.Logging.AddSerilog();

Log.Logger = LoggingConfig.Create("BackgroundJobs")
    .MinimumLevel.Error()
    .CreateLogger();



var host = builder.Build();


using (var scope = host.Services.CreateScope())
{
    var jobRegistrar = scope.ServiceProvider.GetRequiredService<IRecurringJobRegistrar>();
    jobRegistrar.RegisterJobs();
}

host.Run();
