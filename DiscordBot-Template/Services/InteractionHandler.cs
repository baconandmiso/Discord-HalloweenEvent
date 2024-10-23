using System.Reflection;

namespace DiscordBot_Template.Services;

public class InteractionHandler(DiscordSocketClient client, InteractionService interactions, IServiceProvider services, ILogger<InteractionHandler> logger)
{
    public async Task InitializeAsync()
    {
        await interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    
        client.InteractionCreated += OnInteractionCreated;
        interactions.InteractionExecuted += OnInteractionExecuted;
    }

    private async Task OnInteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(client, interaction);
            var result = await interactions.ExecuteCommandAsync(context, services);

            if (!result.IsSuccess)
                _ = Task.Run(() => InteractionExecutedResult(interaction, result));
        }
        catch(Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }

    private Task OnInteractionExecuted(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
            _ = Task.Run(() => InteractionExecutedResult(context.Interaction, result));

        return Task.CompletedTask;
    }

    private async Task InteractionExecutedResult(IDiscordInteraction interaction, IResult result)
    {
        switch (result.Error)
        {
            case InteractionCommandError.UnmetPrecondition:
                logger.LogInformation($"Unmet Precondition - {result.Error}");
                break;
            case InteractionCommandError.BadArgs:
                logger.LogInformation($"Bad Args - {result.Error}");
                break;
            case InteractionCommandError.ConvertFailed:
                logger.LogInformation($"Convert Failed - {result.Error}");
                break;
            case InteractionCommandError.ParseFailed:
                logger.LogInformation($"Parse Failed - {result.Error}");
                break;
            case InteractionCommandError.UnknownCommand:
                logger.LogInformation($"Unknown Command - {result.Error}");
                break;
            case InteractionCommandError.Unsuccessful:
                logger.LogInformation($"UnSuccessful - {result.Error}");
                break;
        }

        if (!interaction.HasResponded)
        {
            await interaction.RespondAsync($"失敗: エラーが発生しました。\n```\n{result.Error}\n```", ephemeral: true);
        }
        else
        {
            await interaction.FollowupAsync($"失敗: エラーが発生しました。\n```\n{result.Error}\n```", ephemeral: true);
        }
    }
}
