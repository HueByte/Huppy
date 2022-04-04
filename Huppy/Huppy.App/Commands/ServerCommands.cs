using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.HuppyCacheService;
using Huppy.Core.Services.PaginatedEmbedService;
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
        private readonly IPaginatorEmbedService _paginatorService;
        public ServerCommands(ILogger<ServerCommands> logger, CacheService cacheService, ICommandLogRepository commandLogRepository, IServerRepository serverRepository, IPaginatorEmbedService paginatorService)
        {
            _logger = logger;
            _commandRepository = commandLogRepository;
            _cacheService = cacheService;
            _serverRepository = serverRepository;
            _paginatorService = paginatorService;
        }

        [SlashCommand("info", "Get current configuration for this server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task GetServerInfo()
        {
            PaginatorDynamicEntry entry = new()
            {
                Name = Guid.NewGuid().ToString(),
                DynamicPages = new()
            };

            PaginatorDynamicPage firstPage = new()
            {
                Name = "Main settings",
                PageNumber = 0,
                Embed = async (IServiceScopeFactory scopeFactory) =>
                {
                    using var scope = scopeFactory.CreateAsyncScope();
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
                    embed.AddField("News enabled", server.AreNewsEnabled, true);
                    embed.AddField("Use Greet", server.UseGreet, true);
                    embed.AddField("Greet message", string.IsNullOrEmpty(server.GreetMessage) ? "`empty`" : server.GreetMessage);
                    var defaultRole = Context.Guild.GetRole(server.RoleID);
                    if (defaultRole is not null)
                        embed.AddField("Default role", defaultRole.Mention, true);

                    return embed.Build();
                }
            };

            PaginatorDynamicPage secondPage = new()
            {
                Name = "Rooms Configuration",
                PageNumber = 1,
                Embed = async (IServiceScopeFactory scopeFactory) =>
                {
                    using var scope = scopeFactory.CreateAsyncScope();
                    var serverRepository = scope.ServiceProvider.GetRequiredService<IServerRepository>();

                    var server = await serverRepository.GetOrCreateAsync(Context);

                    var embed = new EmbedBuilder().WithAuthor(Context.User)
                              .WithColor(Color.DarkPurple)
                              .WithTitle("Rooms Configuration")
                              .WithThumbnailUrl(Context.Guild.IconUrl)
                              .WithFooter("If you want to change server configuration use /configure");

                    embed.AddField("Default room", $"<#{Context.Guild.DefaultChannel.Id}>", true);
                    embed.AddField("Huppy output room", server.Rooms?.OutputRoom > 0 ? $"<#{server.Rooms?.OutputRoom}>" : $"<#{Context.Guild.DefaultChannel.Id}>", true);
                    embed.AddField("Greeting room", server.Rooms?.GreetingRoom > 0 ? $"<#{server.Rooms?.GreetingRoom}>" : $"<#{Context.Guild.DefaultChannel.Id}>", true);
                    embed.AddField("News room", server.Rooms?.NewsOutputRoom > 0 ? $"<#{server.Rooms?.NewsOutputRoom}>" : $"<#{Context.Guild.DefaultChannel.Id}>", true);

                    return embed.Build();
                }
            };

            entry.DynamicPages.Add(firstPage);
            entry.DynamicPages.Add(secondPage);

            await _paginatorService.SendDynamicPaginatedMessage(Context.Interaction, entry, 0);
        }

        [SlashCommand("configure", "Configure Huppy for your server")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureHuppy(bool? UseGreet = null, string? GreetingMessage = null, IRole? DefaultRole = null, bool? EnableNews = false, SocketGuildChannel? HuppyRoom = null, SocketGuildChannel? NewsRoom = null, SocketGuildChannel? GreetingRoom = null)
        {
            var server = await _serverRepository.GetOrCreateAsync(Context);

            server.ServerName = Context.Guild.Name;

            if (EnableNews is not null)
                server.AreNewsEnabled = (bool)EnableNews;

            if (UseGreet is not null)
                server.UseGreet = (bool)UseGreet;

            if (GreetingMessage is not null)
                server.GreetMessage = GreetingMessage;

            if (DefaultRole is not null)
                server.RoleID = DefaultRole.Id;

            if (server.Rooms is not null)
            {
                if (NewsRoom is not null)
                    server.Rooms.NewsOutputRoom = NewsRoom.Id;

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