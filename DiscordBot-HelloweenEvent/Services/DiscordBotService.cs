using Microsoft.Extensions.Hosting;

namespace DiscordBot_Template.Services;

public class DiscordBotService(DiscordSocketClient client, InteractionService interactions, IConfiguration config, ILogger<DiscordBotService> logger, 
    InteractionHandler interactionHandler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        client.Ready += OnReady;
        
        client.Log += Log;
        interactions.Log += Log;

        await interactionHandler.InitializeAsync();

        await client.LoginAsync(TokenType.Bot, config["Secrets:DiscordToken"]);
        await client.StartAsync();

        await Task.Delay(-1, cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (ExecuteTask is null)
            return Task.CompletedTask;

        base.StopAsync(cancellationToken);
        return client.StopAsync();
    }

    public Task Log(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.Information
        };

        logger.Log(severity, message.Exception, $"[{message.Source}] {message.Message}");

        return Task.CompletedTask;
    }

    private Task OnReady()
    {
        _ = Task.Run(async () => {
            try
            {
                #if DEBUG
                    var guild = client.Guilds.FirstOrDefault(x => x.Id.ToString() == config["Discord:GuildId"]);
                    if (guild != null)
                        await interactions.RegisterCommandsToGuildAsync(guild.Id);
                #elif RELEASE
                    await interactions.RegisterCommandsGloballyAsync();
                #endif
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        });

        return Task.CompletedTask;
    }
}
