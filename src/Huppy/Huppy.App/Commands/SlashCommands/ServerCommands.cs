using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.Paginator.Entities;
using Huppy.Kernel.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands.SlashCommands;

[Group("server", "server commands")]
public class ServerCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
{
    private readonly ILogger _logger;
    private readonly IServerService _serverService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IPaginatorService _paginatorService;
    public ServerCommands(ILogger<ServerCommands> logger, IServiceScopeFactory scopeFactory, IPaginatorService paginatorService, IServerService serverService)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _paginatorService = paginatorService;
        _serverService = serverService;
    }

    [SlashCommand("info", "Get current configuration for this server")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task GetServerInfo()
    {
        DynamicPaginatorEntry entry = new(_scopeFactory)
        {
            CurrentPage = 0,
            MessageId = 0,
            Name = "Server info",
            Pages = new(),
            Data = null
        };

        var page1 = async Task<PaginatorPage> (AsyncServiceScope scope, object? data) =>
        {
            var serverRepository = scope.ServiceProvider.GetRequiredService<IServerService>();

            var server = await _serverService.GetOrCreateAsync(Context.Guild.Id, Context.Guild.Name, Context.Guild.DefaultChannel.Id);

            var embed = new EmbedBuilder().WithAuthor(Context.User)
                      .WithColor(Color.DarkPurple)
                      .WithTitle("Main settings")
                      .WithThumbnailUrl(Context.Guild.IconUrl)
                      .WithFooter("If you want to change server configuration use /configure");

            embed.AddField("ID", server.Id, true);
            embed.AddField("Server Name", Context.Guild.Name, true);
            embed.AddField("User count", Context.Guild.MemberCount, true);
            embed.AddField("Use Greet", server.UseGreet, true);
            embed.AddField("Greet message", string.IsNullOrEmpty(server.GreetMessage) ? "`empty`" : server.GreetMessage);

            var defaultRole = Context.Guild.GetRole(server.RoleId);
            if (defaultRole is not null)
                embed.AddField("Default role", defaultRole.Mention, true);

            return new PaginatorPage()
            {
                Embed = embed,
                Page = 0
            };
        };

        var page2 = async Task<PaginatorPage> (AsyncServiceScope scope, object? data) =>
        {
            var serverRepository = scope.ServiceProvider.GetRequiredService<IServerService>();

            var server = await _serverService.GetOrCreateAsync(Context.Guild.Id, Context.Guild.Name, Context.Guild.DefaultChannel.Id);

            var embed = new EmbedBuilder().WithAuthor(Context.User)
                      .WithColor(Color.DarkPurple)
                      .WithTitle("Rooms Configuration")
                      .WithThumbnailUrl(Context.Guild.IconUrl)
                      .WithFooter("If you want to change server configuration use /configure");

            embed.AddField("Huppy output room", server.Rooms?.OutputRoom > 0 ? $"<#{server.Rooms?.OutputRoom}>" : $"<#{Context.Guild.DefaultChannel.Id}>", true);
            embed.AddField("Greeting room", server.Rooms?.GreetingRoom > 0 ? $"<#{server.Rooms?.GreetingRoom}>" : $"<#{Context.Guild.DefaultChannel.Id}>", true);
            embed.AddField("Default room", $"<#{Context.Guild.DefaultChannel.Id}>");

            return new PaginatorPage()
            {
                Embed = embed,
                Page = 1
            };
        };

        entry.Pages.Add(page1);
        entry.Pages.Add(page2);

        await _paginatorService.SendPaginatedMessage(Context.Interaction, entry);
    }

    [SlashCommand("configure", "Configure Huppy for your server")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task ConfigureHuppy(bool? UseGreet = null, string? GreetingMessage = null, IRole? DefaultRole = null, bool? EnableNews = false, SocketGuildChannel? HuppyRoom = null, SocketGuildChannel? NewsRoom = null, SocketGuildChannel? GreetingRoom = null)
    {
        var server = await _serverService.GetOrCreateAsync(Context.Guild.Id, Context.Guild.Name, Context.Guild.DefaultChannel.Id);

        server.ServerName = Context.Guild.Name;

        if (UseGreet is not null)
            server.UseGreet = (bool)UseGreet;

        if (GreetingMessage is not null)
            server.GreetMessage = GreetingMessage;

        if (DefaultRole is not null)
            server.RoleId = DefaultRole.Id;

        if (server.Rooms is not null)
        {
            if (HuppyRoom is not null)
                server.Rooms.OutputRoom = HuppyRoom.Id;

            if (GreetingRoom is not null)
                server.Rooms.GreetingRoom = GreetingRoom.Id;
        }

        await _serverService.UpdateAsync(server);

        var embed = new EmbedBuilder().WithDescription("Updated your server settings\nUse `/server info` command to see current configuration")
                                      .WithThumbnailUrl(Icons.Huppy1)
                                      .WithColor(Color.Magenta)
                                      .WithCurrentTimestamp()
                                      .WithAuthor(Context.User);

        await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
    }
}
