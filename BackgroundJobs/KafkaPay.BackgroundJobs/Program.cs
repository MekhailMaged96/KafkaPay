using Hangfire;
using Hangfire.MemoryStorage;
using KafkaPay.BackgroundJobs;
using KafkaPay.BackgroundJobs.Jobs;
using KafkaPay.BackgroundJobs.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddHangfire(config =>
    config.UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseMemoryStorage()); // Replace with SqlServer, Redis in prod

builder.Services.AddHangfireServer();

builder.Services.AddScoped<ITasksService, TasksService>();
builder.Services.AddSingleton<IRecurringJobRegistrar, RecurringJobsRegistrar>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();


using (var scope = host.Services.CreateScope())
{
    var jobRegistrar = scope.ServiceProvider.GetRequiredService<IRecurringJobRegistrar>();
    jobRegistrar.RegisterJobs();
}

host.Run();
