using Serilog;
using System.Diagnostics;
using System.IO;

//public static class LoggingConfig
//{
//    public static LoggerConfiguration Create(string serviceName)
//    {
//        var logPath = Path.Combine(
//            Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
//            "logs",
//            serviceName,
//            "log-.txt"
//        );

//        return new LoggerConfiguration()
//                   .Enrich.FromLogContext()
//                   .Enrich.WithProperty("Service", serviceName)
//                   .Enrich.WithProperty("TraceId", () =>
//                       Activity.Current?.TraceId.ToString() ?? "none") // Add TraceId
//                   .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Trace={TraceId}] {Message}{NewLine}{Exception}")
//                   .WriteTo.File(
//                       path: logPath,
//                       outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Trace={TraceId}] {Message}{NewLine}{Exception}",
//                       rollingInterval: RollingInterval.Day
//                   );
//    }
//}
