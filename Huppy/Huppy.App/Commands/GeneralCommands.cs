using Discord;
using Discord.Interactions;
using Huppy.Core.Common.HuppyMessages;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.HuppyCacheService;
using Huppy.Core.Services.NewsService;
using Huppy.Core.Services.PaginatorService;
using Huppy.Core.Services.PaginatorService.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class GeneralCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
    {
        private readonly ILogger _logger;
        private readonly ICommandLogRepository _commandRepository;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CacheService _cacheService;
        private readonly INewsApiService _newsService;
        private readonly InteractionService _interactionService;
        private readonly IPaginatorService _paginatorService;
        public GeneralCommands(ILogger<GeneralCommands> logger, CacheService cacheService, ICommandLogRepository commandLogRepository, INewsApiService newsService, InteractionService interactionService, IPaginatorService paginatorService, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _commandRepository = commandLogRepository;
            _cacheService = cacheService;
            _newsService = newsService;
            _interactionService = interactionService;
            _paginatorService = paginatorService;
            _scopeFactory = scopeFactory;
        }

        [SlashCommand("ping", "🏓 pong")]
        public async Task PingCommand()
        {
            await ModifyOriginalResponseAsync((msg) => msg.Content = "Pong");
        }

        [SlashCommand("say", "Says the input message")]
        public async Task SayCommand(string message)
        {
            var orginalMessage = await FollowupAsync(embed: new EmbedBuilder().WithTitle("Sending message...").Build());
            await orginalMessage.DeleteAsync();
            await Context.Channel.SendMessageAsync(text: message);
        }

        [SlashCommand("embed", "Send embed message")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task SendEmbed(string? title = null, string? content = null, bool withAuthor = true, string? thumbnail = null, string? imageUrl = null)
        {
            bool isValid = !(
                   string.IsNullOrEmpty(title)
                && string.IsNullOrEmpty(content)
                && string.IsNullOrEmpty(thumbnail)
                && string.IsNullOrEmpty(imageUrl)
                && !withAuthor);

            if (!isValid)
                throw new Exception("Invalid embed content, at least one parameter has to be fulfilled");

            content = content?.Replace("\\n", "\n");

            var embed = new EmbedBuilder().WithTitle(title ?? "")
                .WithThumbnailUrl(thumbnail ?? "")
                .WithImageUrl(imageUrl ?? "")
                .WithDescription(content ?? "")
                .WithColor(Color.Teal)
                .WithCurrentTimestamp();

            if (withAuthor) embed.WithAuthor(Context.User);

            var orginalMessage = await FollowupAsync(embed: new EmbedBuilder().WithTitle("Sending embed...").Build());
            await orginalMessage.DeleteAsync();
            await Context.Channel.SendMessageAsync(embed: embed.Build());
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
        public async Task Help()
        {
            var pageNames = _paginatorService.GetStaticEmbedsNames(StaticEmbeds.Help.ToString());
            if (pageNames is null)
            {
                throw new Exception("Couldn't find contents of help");
            }

            StaticPaginatorEntry help = new(_scopeFactory)
            {
                CurrentPage = 0,
                MessageId = 0,
                Name = StaticEmbeds.Help.ToString(),
                PageNames = pageNames!
            };

            await _paginatorService.SendPaginatedMessage(Context.Interaction, help);
        }
    }
}