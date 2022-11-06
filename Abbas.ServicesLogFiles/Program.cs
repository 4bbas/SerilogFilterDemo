using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Serilog;
using Serilog.Enrichers;
using Serilog.Events;
using ServicesLogFiles.Services;

namespace ServicesLogFiles;

public class Program
{
    public static void Main(string[] args)
    {
        const string logTemplate = @"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}]<{ThreadId}> [{SourceContext:l}] {Message:lj}{NewLine}{Exception}";
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.With(new ThreadIdEnricher())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Logger(l =>
            {
                l.WriteTo.File("./App_Data/logs/log.txt", LogEventLevel.Information, logTemplate,
                    rollingInterval: RollingInterval.Day, retainedFileCountLimit: 366
                );
                l.Filter.ByExcluding(e => e.Properties.ContainsKey("PeriodicTimeStamp"));
            })
            .WriteTo.Logger(l =>
            {
                l.WriteTo.File("./App_Data/logs/PeriodicTimeStamp_Log.txt", LogEventLevel.Information, logTemplate,
                    rollingInterval: RollingInterval.Day, retainedFileCountLimit: 366
                );
                l.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("PeriodicTimeStamp"));
            });
        Log.Logger = loggerConfig.CreateLogger();

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                Log.Information("====================================================================");
                Log.Information($"Application [{hostContext.HostingEnvironment.ApplicationName}] Starts. " +
                                $"Version: {System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version}; " +
                                $"Environment: {hostContext.HostingEnvironment.EnvironmentName}. ");

                services.AddHostedService<PeriodicWorker>();
                services.AddSingleton<IRepository, Repository>();
            })
            .UseSerilog();
}
