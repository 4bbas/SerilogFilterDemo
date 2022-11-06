using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ServicesLogFiles.Services;

public interface IRepository
{
    Task SaveTimeStamp();
}

public class Repository : IRepository
{
    private readonly ILogger<Repository> _logger;

    public Repository(ILogger<Repository> logger)
    {
        _logger = logger;
    }

    public async Task SaveTimeStamp()
    {
        await Task.Delay(10000);
        _logger.LogInformation("Saving to db...");
    }
}
