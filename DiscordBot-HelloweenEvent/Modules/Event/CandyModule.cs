namespace Modules.Event;

public class CandyModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<CandyModule> _logger;

    private static Dictionary<ulong, Point> _point = new Dictionary<ulong, Point>();

    /// <summary>
    ///     お菓子を奪うのに成功したときの処理
    /// </summary>
    /// <param name="points">与える点数</param>
    /// <param name="targetId">奪われる人</param>
    private async Task SuccesfulSteal(int points, ulong targetId)
    {
        var myPoint = new Point(Context.User.Id, _point.GetValueOrDefault(Context.User.Id)!.Score + points);
        var opponentPoint = new Point(targetId, _point[targetId].Score - points);

        _point[Context.User.Id] = myPoint;
        _point[targetId] = opponentPoint;

        Console.WriteLine($"{Context.User.Id}のスコア: {_point[Context.User.Id].Score}");
        Console.WriteLine($"{targetId}のスコア: {_point[targetId].Score}");

        await FollowupAsync($"お菓子を奪いました。\n**おめでとう！**{points}pt獲得しました。", ephemeral: true);
    }

    /// <summary>
    ///     お菓子を奪うのに失敗したときの処理
    /// </summary>
    /// <param name="points">減らす点数</param>
    /// <returns></returns>
    private async Task FailedSteal(int points)
    {
        var myPoint = new Point(Context.User.Id, _point.GetValueOrDefault(Context.User.Id)!.Score - 4);
        _point[Context.User.Id] = myPoint;

        Console.WriteLine($"{Context.User.Id}のスコア: {_point[Context.User.Id].Score}");

        await FollowupAsync($"お菓子を奪うのに失敗しました。\n**罰ゲーム**\n{points}減点します。", ephemeral: true);
    }

    private bool IsChance(double probability)
    {
        var random = new Random();
        return random.NextDouble() < probability;
    }

    public CandyModule(ILogger<CandyModule> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     お菓子を受け取ってないプレイヤーに対して付与する
    /// </summary>
    [ComponentInteraction("take_candy")]
    public async Task TakeCandy()
    {
        await DeferAsync();

        if (_point.ContainsKey(Context.User.Id))
        {
            await FollowupAsync("お菓子はすでに受け取っています。", ephemeral: true);
            return;
        }

        var point = new Point(Context.User.Id, 50);
        _point.Add(Context.User.Id, point);

        await FollowupAsync("お菓子を受け取りました。", ephemeral: true);
    }

    /// <summary>
    ///     相手からお菓子を奪う
    /// </summary>
    [ComponentInteraction("steal_candy")]
    public async Task StealCandy(string candy)
    {
        var message = (IComponentInteraction)Context.Interaction;

        var random = new Random();
        var number = random.Next(0, _point.Count - 1);
        var targets = _point.Keys.Where(x => x != Context.User.Id).ToArray();

        await message.Message.ModifyAsync(x => x.Components = EventPanel.ComponentBuilder.Build());

        if (!_point.ContainsKey(Context.User.Id))
        {
            await RespondAsync("お菓子を持っていません。", ephemeral: true);
            return;
        }

        if (!_point.TryGetValue(targets[number], out var point))
            return;

        await DeferAsync();

        switch (candy)
        {
            case "very_high_candy":
                if (IsChance(0.3))
                {
                    await SuccesfulSteal(4, targets[number]);
                }
                else
                {
                    await FailedSteal(4);
                }
                break;
            case "high_candy":
                if (IsChance(0.4))
                {
                    await SuccesfulSteal(3, targets[number]);
                }
                else
                {
                    await FailedSteal(3);
                }
                break;
            case "normal_candy":
                if (IsChance(0.6))
                {
                    await SuccesfulSteal(2, targets[number]);
                }
                else
                {
                    await FailedSteal(2);
                }
                break;
            case "low_candy":
                if (IsChance(0.8))
                {
                    await SuccesfulSteal(1, targets[number]);
                }
                else
                {
                    await FailedSteal(2);
                }
                break;
        }
    }
}