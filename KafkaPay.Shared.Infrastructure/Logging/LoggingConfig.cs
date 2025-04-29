using Serilog;
using System.IO;

public static class LoggingConfig
{
    public static LoggerConfiguration Create(string serviceName)
    {
        var logPath = Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
            "logs",
            serviceName,
            "log-.txt"
        );

        return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
            .WriteTo.Console(outputTemplate: "Timestamp:{Timestamp:yyyy-MM-dd HH:mm:ss} Level: {Level:u3} Service: {Service} Message: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: logPath,
                outputTemplate: "Timestamp:{Timestamp:yyyy-MM-dd HH:mm:ss} Level: {Level:u3} Service: {Service} Message: {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                shared: true
            );
    }
}
