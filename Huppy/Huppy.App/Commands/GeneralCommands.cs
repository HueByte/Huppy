using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Common.HuppyMessages;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.GPTService;
using Huppy.Core.Services.HuppyCacheService;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class GeneralCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger _logger;
        private readonly ICommandLogRepository _commandRepository;
        private readonly IServerRepository _serverRepository;
        private readonly CacheService _cacheService;
        public GeneralCommands(ILogger<GeneralCommands> logger, CacheService cacheService, ICommandLogRepository commandLogRepository, IServerRepository serverRepository)
        {
            _logger = logger;
            _commandRepository = commandLogRepository;
            _cacheService = cacheService;
            _serverRepository = serverRepository;
        }

        [SlashCommand("ping", "return pong")]
        public async Task PingCommand()
        {
            await ModifyOriginalResponseAsync((msg) => msg.Content = "Pong");
        }

        [SlashCommand("say", "Says the input message")]
        public async Task SayCommand(string message)
        {
            await ModifyOriginalResponseAsync((msg) => msg.Content = message);
        }

        [SlashCommand("embed", "Send embed message")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task SendEmbed(string title, string content, string? thumbnail = null)
        {
            content = content.Replace("\\n", "\n");

            EmbedBuilder embed = new EmbedBuilder().WithTitle(title)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(thumbnail ?? "https://i.kym-cdn.com/entries/icons/facebook/000/017/618/pepefroggie.jpg")
                .WithDescription(content)
                .WithColor(Color.Teal)
                .WithAuthor(Context.User);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }

        [SlashCommand("whoami", "Hi I'm Huppy!")]
        public async Task AboutMe()
        {
            var embed = new EmbedBuilder().WithTitle("✨ Hello I'm Huppy! ✨")
                                          .WithColor(Color.Teal)
                                          .WithDescription(HuppyBasicMessages.AboutMe)
                                          .WithThumbnailUrl(Icons.Huppy1)
                                          .WithCurrentTimestamp();

            embed.AddField("Users", _cacheService.GetUserNames().Count, true);
            embed.AddField("Commands Used", await _commandRepository.GetCount(), true);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }

        [SlashCommand("server", "Get current configuration for this server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task GetConfiguration()
        {
            var server = await _serverRepository.GetOneAsync(Context.Guild.Id);

            if (server is null)
            {
                server = new()
                {
                    ID = Context.Guild.Id,
                    GreetMessage = "",
                    OutputRoom = Context.Guild.DefaultChannel.Id,
                    UseGreet = false,
                    ServerName = Context.Guild.Name
                };

                await _serverRepository.AddOneAsync(server);
            }

            var embed = new EmbedBuilder().WithAuthor(Context.User)
                                          .WithColor(Color.DarkPurple)
                                          .WithTitle($"Current configuration of {Context.Guild.Name}")
                                          .WithThumbnailUrl(Context.Guild.IconUrl)
                                          .WithFooter("If you want to change server configuration use /configure");

            embed.AddField("ID", server.ID, true);
            embed.AddField("Server Name", Context.Guild.Name, true);
            embed.AddField("User count", Context.Guild.MemberCount, true);
            embed.AddField("Huppy output room", $"<#{server.OutputRoom}>", true);
            embed.AddField("Default channel", Context.Guild.DefaultChannel.Name, true);
            embed.AddField("Use Greet", server.UseGreet, true);
            embed.AddField("Greet message", string.IsNullOrEmpty(server.GreetMessage) ? "`empty`" : server.GreetMessage);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }

        [SlashCommand("configure", "Configure Huppy for your server")]
        public async Task ConfigureHuppy(bool UseGreet, string? GreetingMessage, SocketGuildChannel? HuppyRoom = null)
        {
            // TODO put this into server repository
            var server = await _serverRepository.GetOneAsync(Context.Guild.Id);
            if (server is not null)
            {
                server.ServerName = Context.Guild.Name;
                server.UseGreet = UseGreet;
                server.GreetMessage = GreetingMessage;

                if (HuppyRoom is not null)
                    server.OutputRoom = HuppyRoom.Id;


                await _serverRepository.UpdateOne(server);
            }
            else
            {
                server = new()
                {
                    ID = Context.Guild.Id,
                    GreetMessage = GreetingMessage ?? "",
                    OutputRoom = HuppyRoom?.Id ?? Context.Guild.DefaultChannel.Id,
                    UseGreet = UseGreet,
                    ServerName = Context.Guild.Name
                };

                await _serverRepository.AddOneAsync(server);
            }

            var embed = new EmbedBuilder().WithDescription("Updated your server settings")
                                          .WithThumbnailUrl(Icons.Huppy1)
                                          .WithColor(Color.Magenta)
                                          .WithCurrentTimestamp()
                                          .WithAuthor(Context.User);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }
    }
}