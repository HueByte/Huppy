using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.HuppyCacheService;
using Huppy.Core.Services.PaginatorService;
using Huppy.Core.Services.PaginatorService.Entities;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPaginatorService _paginatorService;
        public ServerCommands(ILogger<ServerCommands> logger, CacheService cacheService, ICommandLogRepository commandLogRepository, IServerRepository serverRepository, IServiceScopeFactory scopeFactory, IPaginatorService paginatorService)
        {
            _logger = logger;
            _commandRepository = commandLogRepository;
            _cacheService = cacheService;
            _serverRepository = serverRepository;
            _scopeFactory = scopeFactory;
            _paginatorService = paginatorService;
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
            };

            Func<AsyncServiceScope, Task<PaginatorPage>> page1 = async (scope) =>
            {
                var serverRepository = scope.ServiceProvider.GetRequiredService<IServerRepository>();

                var server = await serverRepository.GetOrCreateAsync(Context);

                var embed = new EmbedBuilder().WithAuthor(Context.User)
                          .WithColor(Color.DarkPurple)
                          .WithTitle("Main settings")
                          .WithThumbnailUrl(Context.Guild.IconUrl)
                          .WithFooter("If you want to change server configuration use /configure");

                embed.AddField("ID", server.ID, true);
                embed.AddField("Server Name", Context.Guild.Name, true);
                embed.AddField("User count", Context.Guild.MemberCount, true);
                embed.AddField("Use Greet", server.UseGreet, true);
                embed.AddField("Greet message", string.IsNullOrEmpty(server.GreetMessage) ? "`empty`" : server.GreetMessage);

                var defaultRole = Context.Guild.GetRole(server.RoleID);
                if (defaultRole is not null)
                    embed.AddField("Default role", defaultRole.Mention, true);

                return new PaginatorPage()
                {
                    Embed = embed,
                    Page = 0
                };
            };

            Func<AsyncServiceScope, Task<PaginatorPage>> page2 = async (scope) =>
            {
                var serverRepository = scope.ServiceProvider.GetRequiredService<IServerRepository>();

                var server = await serverRepository.GetOrCreateAsync(Context);

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
            var server = await _serverRepository.GetOrCreateAsync(Context);

            server.ServerName = Context.Guild.Name;

            if (UseGreet is not null)
                server.UseGreet = (bool)UseGreet;

            if (GreetingMessage is not null)
                server.GreetMessage = GreetingMessage;

            if (DefaultRole is not null)
                server.RoleID = DefaultRole.Id;

            if (server.Rooms is not null)
            {
                if (HuppyRoom is not null)
                    server.Rooms.OutputRoom = HuppyRoom.Id;

                if (GreetingRoom is not null)
                    server.Rooms.GreetingRoom = GreetingRoom.Id;
            }

            await _serverRepository.UpdateOne(server);

            var embed = new EmbedBuilder().WithDescription("Updated your server settings\nUse `/server info` command to see current configuration")
                                          .WithThumbnailUrl(Icons.Huppy1)
                                          .WithColor(Color.Magenta)
                                          .WithCurrentTimestamp()
                                          .WithAuthor(Context.User);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }
    }
}