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
        public GeneralCommands(ILogger<GeneralCommands> logger, CacheService cacheService, ICommandLogRepository commandLogRepository, INewsApiService newsService, InteractionService interactionService)
        {
            _logger = logger;
            _commandRepository = commandLogRepository;
            _cacheService = cacheService;
            _newsService = newsService;
            _interactionService = interactionService;
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

            // var embed = new EmbedBuilder().WithTitle("test");
            // var component = new ComponentBuilder().WithButton("◀", "help-left").WithButton("▶", "help-right");

            // await ModifyOriginalResponseAsync((msg) =>
            // {
            //     msg.Embed = embed.Build();
            //     msg.Components = component.Build();
            // });
        }

        [SlashCommand("test", "paginator test")]
        public async Task PaginatorTest()
        {
            var embed = new EmbedBuilder().WithTitle("Paginator test")
                                          .WithColor(Color.DarkRed);

            var component = new ComponentBuilder().WithButton("◀", "help-left").WithButton("▶", "help-right");

            var result = await ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = embed.Build();
                msg.Components = component.Build();
            });

            await _cacheService.AddPaginatedMessage(result.Id, new PaginatedMessage(result.Id, 0));
        }

        [ComponentInteraction("help-left")]
        public async Task HelpLeft()
        {
            _logger.LogInformation("Left invoked");
        }

        [ComponentInteraction("help-right")]
        public async Task HelpRight()
        {
            var msg = (Context.Interaction as SocketMessageComponent);
            var test = await _cacheService.GetPaginatedMessage(msg!.Message!.Id);
            await _cacheService.UpdatePaginatedMessage(msg!.Message!.Id, new PaginatedMessage(msg.Message.Id, (byte)(test!.CurrentPage + 1)));

            var embed = new EmbedBuilder().WithTitle(test.CurrentPage.ToString());

            await ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = embed.Build();
            });
        }
    }
}