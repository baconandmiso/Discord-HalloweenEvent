using Microsoft.Extensions.DependencyInjection;

namespace Common.Throttle;

[AttributeUsage(AttributeTargets.Class)]
public class ThrottleGroupAttribute : PreconditionAttribute
{
    public readonly ThrottleBy ThrottleBy;

    public readonly int Limit;

    public readonly int IntervalSeconds;

    public ThrottleGroupAttribute(ThrottleBy throttleBy, int limit, int intervalSeconds)
    {
        ThrottleBy = throttleBy;
        Limit = limit;
        IntervalSeconds = intervalSeconds;
    }

    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var throttleService = services.GetRequiredService<IThrottleService>();
        if (throttleService.CheckThrottle(ThrottleBy, Limit, IntervalSeconds, context.User, context.Guild))
            return PreconditionResult.FromSuccess();

        var reset = throttleService.GetThrottleReset(ThrottleBy, Limit, IntervalSeconds, context.User, context.Guild);
        await context.Interaction.RespondAsync($"クールダウン中です。 **{reset.TotalSeconds:F0}秒後**に実行してください。", ephemeral: true);
        return PreconditionResult.FromError("Throttle exceeded");
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class ThrottleCommandAttribute : PreconditionAttribute
{
    public readonly ThrottleBy ThrottleBy;

    public readonly int Limit;

    public readonly int IntervalSeconds;

    public ThrottleCommandAttribute(ThrottleBy throttleBy, int limit, int intervalSeconds)
    {
        ThrottleBy = throttleBy;
        Limit = limit;
        IntervalSeconds = intervalSeconds;
    }

    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var throttleService = services.GetRequiredService<IThrottleService>();
        var command = commandInfo.Name;

        if (throttleService.CheckThrottle(ThrottleBy, Limit, IntervalSeconds, context.User, context.Guild, command))
            return PreconditionResult.FromSuccess();

        var reset = throttleService.GetThrottleReset(ThrottleBy, Limit, IntervalSeconds, context.User, context.Guild, command);
        return PreconditionResult.FromError($"クールダウン中です。 **{reset.TotalSeconds:F0}秒後**に実行してください。");
    }
}
