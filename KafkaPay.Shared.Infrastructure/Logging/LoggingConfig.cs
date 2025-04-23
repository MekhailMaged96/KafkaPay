using Serilog;
using System.IO;

public static class LoggingConfig
{
    public static LoggerConfiguration Create(string serviceName)
    {
        // Centralized log path (solution-level "logs" folder)
        var logPath = Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, // Go up to solution root
            "logs",
            serviceName,
            "log-.txt"
        );

        return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {Service}: {Message}{NewLine}{Exception}")
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Service} {Message}{NewLine}{Exception}"
            );
    }
}