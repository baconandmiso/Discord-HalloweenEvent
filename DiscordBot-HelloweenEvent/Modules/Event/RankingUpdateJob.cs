using DiscordBot_HelloweenEvent.Database;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Modules.Event;

public class RankingUpdateJob : IJob
{
    private readonly IServiceProvider _provider;

    private readonly IConfiguration _config;

    private readonly DiscordBotDBContext _dbContext;

    public RankingUpdateJob(IServiceProvider provider, IConfiguration configuration, DiscordBotDBContext dbContext)
    {
        _provider = provider;
        _config = configuration;
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var client = _provider.GetRequiredService<DiscordSocketClient>();

        if (!ulong.TryParse(_config["Discord:GuildId"], out var guildId) || !ulong.TryParse(_config["Discord:ChannelId"], out var channelId))
        {
            throw new Exception("Discord:GuildId または Discord:ChannelIdの値が不正です。");
        }

        var messages = await client.GetGuild(guildId).GetTextChannel(channelId).GetMessagesAsync(0, Direction.After, 1).FlattenAsync();
        var message = messages.FirstOrDefault() ?? throw new Exception("メッセージが見つかりません。");

        var points = _dbContext.EventPoints.Where(x => x.IsListedRanking);
        var ranking = points.OrderByDescending(x => x.Score).Take(10).ToList();

        var ranking_str = new string[ranking.Count];
        for (var i = 0; i < ranking.Count; i++)
        {
            var user = client.GetUser(ranking[i].UserId);
            ranking_str[i] = $"{i + 1}位: {user.Mention} スコア: {ranking[i].Score}pt";
        }

        var embedAuthorBuilder = new EmbedAuthorBuilder()
            .WithName("👻 ハロウィンイベント 2024🎃");

        var embedBuilder = new EmbedBuilder()
            .WithTitle("ランキング TOP10")
            .WithDescription(string.Join('\n', ranking_str))
            .WithAuthor(embedAuthorBuilder)
            .WithFooter($"{DateTime.Now:yyyy年MM月dd日 HH時mm分}時点のデータです。")
            .WithColor(Color.DarkPurple);

        await message.Channel.ModifyMessageAsync(message.Id, x => x.Embed = embedBuilder.Build());

        Console.WriteLine("ランキングを更新しました。");
    }
}
