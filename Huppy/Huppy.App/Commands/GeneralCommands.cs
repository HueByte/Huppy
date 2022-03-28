using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Common.HuppyMessages;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.HuppyCacheService;
using Huppy.Core.Services.NewsService;
using Huppy.Core.Services.PaginatedEmbedService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class GeneralCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger _logger;
        private readonly ICommandLogRepository _commandRepository;
        private readonly CacheService _cacheService;
        private readonly INewsApiService _newsService;
        private readonly InteractionService _interactionService;
        private readonly IPaginatorEmbedService _paginatorEmbedService;
        public GeneralCommands(ILogger<GeneralCommands> logger, CacheService cacheService, ICommandLogRepository commandLogRepository, INewsApiService newsService, InteractionService interactionService, IPaginatorEmbedService paginatorEmbedService)
        {
            _logger = logger;
            _commandRepository = commandLogRepository;
            _cacheService = cacheService;
            _newsService = newsService;
            _interactionService = interactionService;
            _paginatorEmbedService = paginatorEmbedService;
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
        public async Task SendEmbed(string title, string content, bool withAuthor = true, string? thumbnail = null)
        {
            content = content.Replace("\\n", "\n");

            EmbedBuilder embed = new EmbedBuilder().WithTitle(title)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(thumbnail ?? "")
                .WithDescription(content)
                .WithColor(Color.Teal);

            if (withAuthor)
                embed.WithAuthor(Context.User);

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

        [SlashCommand("help", "Display help information")]
        public async Task PaginatorTestNew()
        {
            // Get Paginated entry by name 
            var help = _paginatorEmbedService.GetStaticPaginatorEntries()
                                             .FirstOrDefault(e => e.Name == PaginatorEntriesNames.Help);

            if (help is null)
                throw new Exception("Didn't find contents of help");

            await _paginatorEmbedService.SendStaticPaginatedMessage(Context.Interaction, help, 0);
        }
    }
}