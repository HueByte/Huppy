using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Huppy.App.Commands.SlashCommands;

[Group("avatar", "Avatar commands")]
public class AvatarCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
{
    private readonly DiscordShardedClient _client;
    public AvatarCommands(DiscordShardedClient client)
    {
        _client = client;
    }

    [SlashCommand("server", "gets server icon")]
    public async Task GetServerIconAsync()
    {
        var icon = Context.Guild.IconUrl;
        var embed = new EmbedBuilder().WithTitle(Context.Guild.Name)
            .WithImageUrl(icon);

        await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
    }

    [SlashCommand("user", "gets user avatar")]
    public async Task GetServerIconAsync(IUser? user = null)
    {
        user ??= Context.User;
        var embed = new EmbedBuilder();

        var icon = user.GetAvatarUrl(size: 256);
        embed.WithAuthor(user)
            .WithImageUrl(icon);

        await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
    }
}