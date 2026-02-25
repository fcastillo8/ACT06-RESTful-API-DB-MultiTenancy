using Microsoft.Extensions.Hosting;
using Serilog;

namespace Infrastructure.Logging
{
    /// <summary>
    /// Serilog configuration for the application.
    /// Logs to both console and file.
    /// </summary>
    public static class SerilogConfig
    {
        public static void ConfigureSerilog(IHostBuilder hostBuilder)
        {
            hostBuilder.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "MultiTenantApi")
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .WriteTo.File(
                        path: "logs/log-.txt",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            });
        }
    }
}
