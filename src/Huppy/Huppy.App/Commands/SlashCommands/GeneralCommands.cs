using Discord;
using Discord.Interactions;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.HuppyCacheStorage;
using Huppy.Core.Services.Paginator.Entities;
using Huppy.Kernel;
using Huppy.Kernel.Constants;
using Huppy.Kernel.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands.SlashCommands;

public class GeneralCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
{
    private readonly ILogger _logger;
    private readonly ICommandLogService _commandLogService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CacheStorageService _cacheService;
    private readonly InteractionService _interactionService;
    private readonly IPaginatorService _paginatorService;
    public GeneralCommands(ILogger<GeneralCommands> logger, ICommandLogService commandLogService, CacheStorageService cacheService, InteractionService interactionService, IPaginatorService paginatorService, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _commandLogService = commandLogService;
        _cacheService = cacheService;
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
        embed.AddField("Commands Used", await _commandLogService.GetCount(), true);

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
