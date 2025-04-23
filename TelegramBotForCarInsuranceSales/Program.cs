using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<BotService>();
        services.AddSingleton<AIConversationService>();
        services.AddHostedService<BotWorker>();
    })
    .Build();

await host.RunAsync();