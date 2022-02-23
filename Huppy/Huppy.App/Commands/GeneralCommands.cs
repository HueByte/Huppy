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
        private readonly CacheService _cacheService;
        public GeneralCommands(ILogger<GeneralCommands> logger, CacheService cacheService, ICommandLogRepository commandLogRepository)
        {
            _logger = logger;
            _commandRepository = commandLogRepository;
            _cacheService = cacheService;
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
    }
}