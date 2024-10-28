using Common.Throttle;
using DiscordBot_HelloweenEvent.Database;

namespace Modules.Event;

public class RankingModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<RankingModule> _logger;

    private readonly DiscordBotDBContext _dbContext;

    public RankingModule(ILogger<RankingModule> logger, DiscordBotDBContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [RequireOwner]
    [SlashCommand("ranking-create", "ランキングを作成します。")]
    public async Task Create()
    {
        var embedAuthorBuilder = new EmbedAuthorBuilder()
            .WithName("👻 ハロウィンイベント 2024🎃");
            
        var embedBuilder = new EmbedBuilder()
            .WithTitle("ランキング TOP10")
            .WithDescription("データがありません。")
            .WithAuthor(embedAuthorBuilder)
            .WithFooter($"{Context.Interaction.CreatedAt.LocalDateTime.ToString("yyyy年MM月dd日 HH時mm分")}時点のデータです。")
            .WithColor(Color.DarkPurple);

        var componentBuilder = new ComponentBuilder()
            .WithButton("自分の順位とスコアをみる", "ranking-my", ButtonStyle.Primary);

        await DeferAsync();
        await DeleteOriginalResponseAsync();

        await Context.Channel.SendMessageAsync($"ランキング掲載条件\n1. このイベントに参加していること\n2. 最低5回、**お菓子を奪う**のを試みたこと\n\nランキングは10分毎に更新されます。", embed: embedBuilder.Build(), components: componentBuilder.Build());
    }

    [ThrottleCommand(ThrottleBy.User, 1, 70)]
    [ComponentInteraction("ranking-my")]
    public async Task ViewRankingMy()
    {
        await DeferAsync();

        var myPoint = _dbContext.EventPoints.FirstOrDefault(x => x.UserId == Context.User.Id);
        if (myPoint == null)
        {
            await FollowupAsync("データがありません。", ephemeral: true);
            return;
        }

        if (myPoint.IsListedRanking)
        {
            var myRank = _dbContext.EventPoints.Count(x => x.Score > myPoint.Score && x.IsListedRanking) + 1;
            await FollowupAsync($"あなたの順位: {myRank}位, スコア: {myPoint.Score}pt", ephemeral: true);
        }
        else
        {
            await FollowupAsync($"あなたの順位: ランク外, スコア: {myPoint.Score}pt", ephemeral: true);
        }
    }
}
