using Hangfire;
using Hangfire.MemoryStorage;
using KafkaPay.BackgroundJobs;
using KafkaPay.BackgroundJobs.Jobs;
using KafkaPay.BackgroundJobs.Services;
using KafkaPay.Shared.Application;
using KafkaPay.Shared.Infrastructure;
using KafkaPay.Shared.Infrastructure.Backgrounds.Jobs;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddHangfire(config =>
    config.UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseMemoryStorage()); // Replace with SqlServer, Redis in prod

builder.Services.AddHangfireServer();

builder.Services.AddScoped<ITasksService, TasksService>();
builder.Services.AddScoped<ProcessOutboxMessageJob>();
builder.AddInfrastructureServices();
builder.AddApplicationServices();

builder.Services.AddSingleton<IRecurringJobRegistrar, RecurringJobsRegistrar>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();


using (var scope = host.Services.CreateScope())
{
    var jobRegistrar = scope.ServiceProvider.GetRequiredService<IRecurringJobRegistrar>();
    jobRegistrar.RegisterJobs();
}

host.Run();
