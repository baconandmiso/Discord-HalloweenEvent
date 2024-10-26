namespace Modules.Event;

public static class EventPanel
{
    private readonly static SelectMenuBuilder SelectMenuBuilder = new SelectMenuBuilder()
            .WithCustomId("steal_candy")
            .WithPlaceholder("お菓子を奪う")
            .AddOption("すごく高い飴", "very_high_candy", emote: Emoji.Parse("🍭"))
            .AddOption("それなりに高い飴", "high_candy", emote: Emote.Parse("<:candy_1:1299745167385038969>"))
            .AddOption("普通の飴", "normal_candy", emote: Emote.Parse("<:candy_2:1299745926650265701>"))
            .AddOption("安い飴", "low_candy", emote: Emoji.Parse("🍬"));

    public readonly static ComponentBuilder ComponentBuilder = new ComponentBuilder()
            .WithButton("お菓子を受け取る", "take_candy", ButtonStyle.Primary)
            .WithSelectMenu(SelectMenuBuilder);
}
