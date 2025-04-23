using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class BotWorker : BackgroundService
{
    private readonly BotService _botService;

    public BotWorker(BotService botService)
    {
        _botService = botService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _botService.StartAsync(stoppingToken);
    }
}