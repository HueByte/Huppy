using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Common.HuppyMessages;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.HuppyCacheService;
using Huppy.Core.Services.NewsService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class GeneralCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger _logger;
        private readonly ICommandLogRepository _commandRepository;
        private readonly CacheService _cacheService;
        private readonly INewsApiService _newsService;
        public GeneralCommands(ILogger<GeneralCommands> logger, CacheService cacheService, ICommandLogRepository commandLogRepository, INewsApiService newsService)
        {
            _logger = logger;
            _commandRepository = commandLogRepository;
            _cacheService = cacheService;
            _newsService = newsService;
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

        // [SlashCommand("help", "Display help information")]
        // public async Task Help()
        // {
        //     var embed = new EmbedBuilder().WithTitle("test");
        //     var component = new ComponentBuilder().WithButton("◀", "help-left").WithButton("▶", "help-right");

        //     await ModifyOriginalResponseAsync((msg) =>
        //     {
        //         msg.Embed = embed.Build();
        //         msg.Components = component.Build();
        //     });
        // }

        [ComponentInteraction("help-left")]
        public async Task HelpLeft()
        {
            _logger.LogInformation("Left invoked");
        }

        [ComponentInteraction("help-right")]
        public async Task HelpRight()
        {
            var msg = (Context.Interaction as SocketMessageComponent);
            var test = msg.Data.Values.FirstOrDefault();
            _logger.LogInformation("Left invoked");
            var embed = new EmbedBuilder().WithTitle("updated test");

            await ModifyOriginalResponseAsync((msg) =>
            {
                var values = msg.Components.GetValueOrDefault();
                _logger.LogInformation(values.ToString());
                msg.Embed = embed.Build();
            });
        }
    }
}