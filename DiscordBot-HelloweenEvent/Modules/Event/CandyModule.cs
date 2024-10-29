using Common.Throttle;
using DiscordBot_HelloweenEvent.Database;
using DiscordBot_HelloweenEvent.Database.Models;

namespace Modules.Event;

public class CandyModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<CandyModule> _logger;

    private readonly Random _random;

    private readonly DiscordBotDBContext _dbContext;

    /// <summary>
    ///     お菓子を奪うのに成功したときの処理
    /// </summary>
    /// <param name="points">与える点数</param>
    /// <param name="targetId">奪われる人</param>
    private async Task SuccesfulSteal(int points, ulong targetId)
    {
        var myPoints = _dbContext.EventPoints.FirstOrDefault(x => x.UserId == Context.User.Id);
        var opponentPoints = _dbContext.EventPoints.FirstOrDefault(x => x.UserId == targetId);
        if (myPoints == null || opponentPoints == null)
        {
            await FollowupAsync("お菓子を持っているプレイヤーがいません。", ephemeral: true);
            return;
        }

        var myPoint = new Point(Context.User.Id, myPoints.Score); // 自分
        var opponentPoint = new Point(targetId, opponentPoints.Score); // 相手

        var addPoint = new Point(Context.User.Id, points); // 増やす点数
        var subPoint = new Point(targetId, points); // 減らす点数

        var subedOpponentPoint = opponentPoint.Sub(subPoint);
        if (subedOpponentPoint.Score < 0) // 再抽選(MAX=3)
        {
            return;
        }

        var addedMyPoint = myPoint.Add(addPoint);

        myPoints.Score = addedMyPoint.Score;
        opponentPoints.Score = subedOpponentPoint.Score;

        await _dbContext.SaveChangesAsync();

        Console.WriteLine($"{Context.User.Id}のスコア: {myPoints.Score}");
        Console.WriteLine($"{targetId}のスコア: {opponentPoints.Score}");

        await FollowupAsync($"お菓子を奪いました。\n**おめでとう！**{points}pt獲得しました。", ephemeral: true);
    }

    /// <summary>
    ///     お菓子を奪うのに失敗したときの処理
    /// </summary>
    /// <param name="points">減らす点数</param>
    /// <returns></returns>
    private async Task FailedSteal(int points)
    {
        var myPoints = _dbContext.EventPoints.FirstOrDefault(x => x.UserId == Context.User.Id);
        if (myPoints == null)
        {
            await FollowupAsync("お菓子を持っているプレイヤーがいません。", ephemeral: true);
            return;
        }

        var myPoint = new Point(Context.User.Id, myPoints.Score);
        var subPoint = new Point(Context.User.Id, points);
        var subedMyPoint = myPoint.Sub(subPoint);

        myPoints.Score = subedMyPoint.Score;
        await _dbContext.SaveChangesAsync();

        Console.WriteLine($"{Context.User.Id}のスコア: {myPoints.Score}");

        await FollowupAsync($"お菓子を奪うのに失敗しました。\n**罰ゲーム**\n{points}pt減点します。", ephemeral: true);
    }

    /// <summary>
    ///     抽選
    /// </summary>
    private bool IsChance(double probability)
    {
        return _random.NextDouble() < probability;
    }

    public CandyModule(ILogger<CandyModule> logger, DiscordBotDBContext dbContext)
    {
        _random = new Random();
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    ///     お菓子を受け取ってないプレイヤーに対して付与する
    /// </summary>
    [ComponentInteraction("take_candy")]
    public async Task TakeCandy()
    {
        await DeferAsync();

        if (_dbContext.EventPoints.FirstOrDefault(x => x.UserId == Context.User.Id) != null)
        {
            await FollowupAsync("お菓子をすでに受け取っています。", ephemeral: true);
            return;
        }

        var point = new Point(Context.User.Id, 50);
        _dbContext.EventPoints.Add(new EventPoint { UserId = point.UserId, Score = point.Score });
        await _dbContext.SaveChangesAsync();

        await FollowupAsync("お菓子を受け取りました。", ephemeral: true);
    }

    /// <summary>
    ///     相手からお菓子を奪う
    /// </summary>
    [ThrottleCommand(ThrottleBy.User, 3, 180)]
    [ComponentInteraction("steal_candy")]
    public async Task StealCandy(string candy)
    {
        await DeferAsync();

        var message = (IComponentInteraction)Context.Interaction;
        await message.Message.ModifyAsync(x => x.Components = EventPanel.ComponentBuilder.Build());

        if (_dbContext.EventPoints.FirstOrDefault(x => x.UserId == Context.User.Id) == null)
        {
            await FollowupAsync("お菓子を持っていません。", ephemeral: true);
            return;
        }

        var targets = _dbContext.EventPoints.Where(x => x.UserId != Context.User.Id && x.Score > 4);
        var number = _random.Next(0, targets.Count());
        if (!targets.Any())
        {
            await FollowupAsync("自分以外にお菓子を持っているプレイヤーがいません。", ephemeral: true);
            return;
        }

        var myPoints = _dbContext.EventPoints.First(x => x.UserId == Context.User.Id);
        if (!myPoints.IsListedRanking)
            myPoints.StealCount++;

        if (myPoints.StealCount >= 3)
        {
            myPoints.IsListedRanking = true;
        }

        _dbContext.SaveChanges();

        var targetId = targets.ToArray()[number].UserId;

        switch (candy)
        {
            case "very_high_candy":
                if (IsChance(0.3))
                {
                    await SuccesfulSteal(4, targetId);
                }
                else
                {
                    await FailedSteal(4);
                }
                break;
            case "high_candy":
                if (IsChance(0.4))
                {
                    await SuccesfulSteal(3, targetId);
                }
                else
                {
                    await FailedSteal(3);
                }
                break;
            case "normal_candy":
                if (IsChance(0.6))
                {
                    await SuccesfulSteal(2, targetId);
                }
                else
                {
                    await FailedSteal(2);
                }
                break;
            case "low_candy":
                if (IsChance(0.8))
                {
                    await SuccesfulSteal(1, targetId);
                }
                else
                {
                    await FailedSteal(2);
                }
                break;
        }
    }
}