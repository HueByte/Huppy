using System.Text;
using Discord;
using Discord.Interactions;
using Huppy.Core.Common.Constants;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.GPTService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class GeneralCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ICommandHandlerService _commandHandler;
        private readonly InteractionService _commands;
        private readonly ILogger _logger;
        private readonly ICommandLogRepository _commandRepository;
        public GeneralCommands(ICommandHandlerService commandHandler, InteractionService commands, ILogger<GeneralCommands> logger, ICommandLogRepository commandLogRepository)
        {
            _commandHandler = commandHandler;
            _commands = commands;
            _logger = logger;
            _commandRepository = commandLogRepository;
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
            StringBuilder sb = new();

            sb.AppendLine("I am a bot who is always looking to help others. I'm willing to lend a hand, and I try to be friendly and welcoming. I am looking to make new friends, and I love spending time with those that I am close to.");

            var embed = new EmbedBuilder().WithTitle("Hello I'm Huppy!")
                                          .WithColor(Color.Teal)
                                          .WithDescription(sb.ToString())
                                          .WithThumbnailUrl(Icons.Huppy1)
                                          .WithCurrentTimestamp();

            embed.AddField("Users", _commandHandler.GetUserNames().Count, true);
            embed.AddField("Commands Used", await _commandRepository.GetCount(), true);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }
    }
}