using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.HuppyCacheService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    [Group("server", "server commands")]
    public class ServerCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger _logger;
        private readonly ICommandLogRepository _commandRepository;
        private readonly IServerRepository _serverRepository;
        private readonly CacheService _cacheService;
        public ServerCommands(ILogger<ServerCommands> logger, CacheService cacheService, ICommandLogRepository commandLogRepository, IServerRepository serverRepository)
        {
            _logger = logger;
            _commandRepository = commandLogRepository;
            _cacheService = cacheService;
            _serverRepository = serverRepository;
        }

        [SlashCommand("info", "Get current configuration for this server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task GetServerInfo()
        {
            var server = await _serverRepository.GetOrCreateAsync(Context);

            var embed = new EmbedBuilder().WithAuthor(Context.User)
                                          .WithColor(Color.DarkPurple)
                                          .WithTitle($"Current configuration of {Context.Guild.Name}")
                                          .WithThumbnailUrl(Context.Guild.IconUrl)
                                          .WithFooter("If you want to change server configuration use /configure");

            embed.AddField("ID", server.ID, true);
            embed.AddField("Server Name", Context.Guild.Name, true);
            embed.AddField("User count", Context.Guild.MemberCount, true);
            embed.AddField("Huppy output room", $"<#{server.OutputRoom}>", true);
            embed.AddField("Default room", $"<#{Context.Guild.DefaultChannel.Id}>", true);
            embed.AddField("News room", server.NewsOutputRoom > 0 ? $"<#{server.NewsOutputRoom}>" : $"<#{Context.Guild.DefaultChannel.Id}>", true);
            embed.AddField("News enabled", server.AreNewsEnabled, true);
            embed.AddField("Use Greet", server.UseGreet, true);
            embed.AddField("Greet message", string.IsNullOrEmpty(server.GreetMessage) ? "`empty`" : server.GreetMessage);

            var defaultRole = Context.Guild.GetRole(server.RoleID);
            if (defaultRole is not null)
                embed.AddField("Default role", defaultRole.Mention, true);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }

        [SlashCommand("configure", "Configure Huppy for your server")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureHuppy(bool? UseGreet = null, string? GreetingMessage = null, IRole? DefaultRole = null, SocketGuildChannel? HuppyRoom = null, bool? EnableNews = false, SocketGuildChannel? NewsRoom = null)
        {
            var server = await _serverRepository.GetOrCreateAsync(Context);

            server.ServerName = Context.Guild.Name;

            if (EnableNews is not null)
                server.AreNewsEnabled = (bool)EnableNews;

            if (NewsRoom is not null)
                server.NewsOutputRoom = NewsRoom.Id;

            if (UseGreet is not null)
                server.UseGreet = (bool)UseGreet;

            if (GreetingMessage is not null)
                server.GreetMessage = GreetingMessage;

            if (HuppyRoom is not null)
                server.OutputRoom = HuppyRoom.Id;

            if (DefaultRole is not null)
                server.RoleID = DefaultRole.Id;

            await _serverRepository.UpdateOne(server);

            var embed = new EmbedBuilder().WithDescription("Updated your server settings\nUse `/server` command to see current configuration")
                                          .WithThumbnailUrl(Icons.Huppy1)
                                          .WithColor(Color.Magenta)
                                          .WithCurrentTimestamp()
                                          .WithAuthor(Context.User);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }
    }
}