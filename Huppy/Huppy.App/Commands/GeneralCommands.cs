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
        public async Task Help()
        {
            var commandGroups = _interactionService.Modules.OrderBy(e => e.SlashCommands.Count).ToList();

            StringBuilder sb = new();
            foreach (var group in commandGroups)
            {
                if (!(group.SlashCommands.Count > 0))
                    continue;

                sb.AppendLine($"> 🔰 **{(string.IsNullOrEmpty(group.SlashGroupName) ? "Other" : group.SlashGroupName)}**");
                foreach (var command in group.SlashCommands)
                {
                    sb.AppendLine($"- *{command.Name}*: {command.Description}");
                }
                sb.AppendLine("\n");
            }

            var embed = new EmbedBuilder().WithTitle("Help")
                                          .WithColor(Color.Teal)
                                          .WithThumbnailUrl(Icons.Huppy1)
                                          .WithDescription(sb.ToString());

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }

        [SlashCommand("test", "paginator test")]
        public async Task PaginatorTest()
        {
            // var embed = new EmbedBuilder().WithTitle("Paginator test")
            //                               .WithColor(Color.DarkRed);

            // var component = new ComponentBuilder().WithButton("◀", "help-left").WithButton("▶", "help-right");

            // var result = await ModifyOriginalResponseAsync((msg) =>
            // {
            //     msg.Embed = embed.Build();
            //     msg.Components = component.Build();
            // });

            // await _cacheService.AddPaginatedMessage(result.Id, new PaginatedMessage(result.Id, 0));
        }

        [SlashCommand("test2", "paginator test")]
        public async Task PaginatorTestNew()
        {
            var help = _paginatorEmbedService.GetPaginatorEntries()
                                             .FirstOrDefault(e => e.Name == PaginatorEntriesNames.Help);

            var component = new ComponentBuilder().WithButton("◀", "paginator-left").WithButton("▶", "paginator-right");

            var result = await ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = help.Pages.First().Embed;
                msg.Components = component.Build();
            });

            await _cacheService.AddPaginatedMessage(result.Id, new PaginatedMessage(result.Id, 0, PaginatorEntriesNames.Help));
        }

        [ComponentInteraction("paginator-left")]
        public async Task HelpLeft()
        {
            var msg = (Context.Interaction as SocketMessageComponent);
            var cacheMessage = await _cacheService.GetPaginatedMessage(msg.Message.Id);
            var paginatedMessage = _paginatorEmbedService.GetPaginatorEntries()
                                                         .FirstOrDefault(e => e.Name == cacheMessage.EntryName);

            var component = new ComponentBuilder().WithButton("◀", "paginator-left").WithButton("▶", "paginator-right");


            if (cacheMessage is null)
                return;

            if (cacheMessage.CurrentPage == 0 && cacheMessage.CurrentPage >= paginatedMessage.Pages.Count)
                return;

            ushort currentPage = (byte)(cacheMessage.CurrentPage - 1);

            var result = await ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = paginatedMessage!.Pages[currentPage].Embed;
                msg.Components = component.Build();
            });

            await _cacheService.UpdatePaginatedMessage(result.Id, new PaginatedMessage(result.Id, currentPage, cacheMessage.EntryName));
        }

        [ComponentInteraction("paginator-right")]
        public async Task HelpRight()
        {
            var msg = (Context.Interaction as SocketMessageComponent);
            var cacheMessage = await _cacheService.GetPaginatedMessage(msg.Message.Id);
            var paginatedMessage = _paginatorEmbedService.GetPaginatorEntries()
                                                         .FirstOrDefault(e => e.Name == cacheMessage.EntryName);

            var component = new ComponentBuilder().WithButton("◀", "paginator-left").WithButton("▶", "paginator-right");


            if (cacheMessage is null)
                return;

            if (cacheMessage.CurrentPage == 0 && cacheMessage.CurrentPage >= paginatedMessage.Pages.Count)
                return;

            ushort currentPage = (byte)(cacheMessage.CurrentPage + 1);

            var result = await ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = paginatedMessage!.Pages[currentPage].Embed;
                msg.Components = component.Build();
            });

            await _cacheService.UpdatePaginatedMessage(result.Id, new PaginatedMessage(result.Id, currentPage, cacheMessage.EntryName));
        }
    }
}