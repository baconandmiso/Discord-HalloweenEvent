namespace Modules.Event;

[DefaultMemberPermissions(GuildPermission.Administrator)]
[Group("eventpanel", "イベントパネルに関するコマンドです。")]
public class EventPanelModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<EventPanelModule> _logger;

    public EventPanelModule(ILogger<EventPanelModule> logger)
    {
        _logger = logger;
    }

    [SlashCommand("create", "イベントパネルを作成します。")]
    public async Task Create()
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle("👻 ハロウィンイベント 2024 🎃")
            .WithDescription("### 遊び方 \n" +
                            "1. **お菓子を受け取る**というボタンを押してお菓子を受け取ってください。\n" +
                            " これで、参加完了です。最初の持ち点は50点です。\n" +
                            "2. **お菓子を奪う**というメニューより、獲得したいお菓子を選んでください。\n" +
                            " ※ それぞれ、確率と点数が異なります。 点数が低い順から80%, 60%, 40%, 30%となります。\n" +
                            " ※ 奪いに行く相手はランダムです。もし奪われた場合、一定の点数、減点されます。\n" +
                            " ※ 2回行う毎に**3分の**クールダウンが入ります。\n" +
                            "3. お菓子を奪うのに失敗したら、ニックネームが変更されます。(ランダムです。名前は戻せます。)\n" +
                            "4. 持ち点が0になってもゲームオーバーにはなりません。ご安心ください。\n\n" +
                            "### 結果発表等について\n" +
                            "11/1の夜に行いたいと思います。 景品等は後日お知らせします。");

        await DeferAsync();
        await DeleteOriginalResponseAsync();
        await Context.Channel.SendMessageAsync(embed: embedBuilder.Build(), components: EventPanel.ComponentBuilder.Build());
    }
}
