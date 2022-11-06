using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Context;
using System;
using Microsoft.Extensions.DependencyInjection;
using ServicesLogFiles.Services;

namespace ServicesLogFiles;

public class PeriodicWorker : BackgroundService
{
    private readonly ILogger<PeriodicWorker> _logger;
    private readonly IServiceScopeFactory _serviceFactory;
    private readonly TimeSpan _interval;

    public PeriodicWorker(ILogger<PeriodicWorker> logger, IServiceScopeFactory serviceFactory)
    {
        _logger = logger;
        _serviceFactory = serviceFactory;
        _interval = TimeSpan.FromSeconds(2);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PeriodicTimer rerunInternal = new PeriodicTimer(_interval);
        using (LogContext.PushProperty("PeriodicTimeStamp", 1))
        {
            while (!stoppingToken.IsCancellationRequested &&
                await rerunInternal.WaitForNextTickAsync(stoppingToken))
            {
                await using AsyncServiceScope serviceScope = _serviceFactory.CreateAsyncScope();
                await Task.Run(() =>
                {
                    SaveTimeStamp(serviceScope.ServiceProvider.GetRequiredService<IRepository>());
                });
            }
        }
        await Task.CompletedTask;
    }

    private async void SaveTimeStamp(IRepository repo)
    {
        await repo.SaveTimeStamp();
        _logger.LogInformation("Saved!");
    }
}
