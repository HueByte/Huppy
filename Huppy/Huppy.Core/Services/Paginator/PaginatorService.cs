using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Interfaces;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.HuppyCache;
using Huppy.Core.Services.Paginator.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.Paginator;

public class PaginatorService : IPaginatorService
{
    private readonly CacheService _cacheService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaginatorService> _logger;
    private Dictionary<string, List<PaginatorPage>> _staticEmbeds;

    public PaginatorService(ILogger<PaginatorService> logger, CacheService cacheService, InteractionService interactionService, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _cacheService = cacheService;
        _scopeFactory = scopeFactory;
        _staticEmbeds = new();
    }

    public void AddStaticEmbed(string key, List<PaginatorPage> pages)
    {
        _staticEmbeds.TryAdd(key, pages);
    }

    public Task RegisterStaticEmbeds(Dictionary<string, List<PaginatorPage>> embeds)
    {
        _staticEmbeds = new(embeds);
        foreach ((var key, var pages) in _staticEmbeds)
        {
            _logger.LogInformation("Static Embed Registered: [{key}] - [{pages}]", key, string.Join(", ", pages.Select(e => e.Name)));
        }

        return Task.CompletedTask;
    }

    public List<PaginatorPage>? GetStaticEmbeds(string keyName)
    {
        _staticEmbeds.TryGetValue(keyName, out List<PaginatorPage>? result);
        return result;
    }

    public List<string?>? GetStaticEmbedsNames(string keyName)
    {
        _staticEmbeds.TryGetValue(keyName, out var result);
        return result?.Select(e => e.Name).ToList();
    }

    public async Task SendPaginatedMessage(SocketInteraction interaction, IPaginatorEntry paginatorEntry)
    {
        var messageId = await ExecutePaginator(interaction, paginatorEntry, 0);
        if (messageId == 0)
            return;

        paginatorEntry.MessageId = messageId;
        await _cacheService.AddPaginatedEntry(messageId, paginatorEntry);
    }

    public Task<DynamicPaginatorEntry> GeneratePaginatorEntry(IInteractionContext context, int elementsCount, int pageSize, Func<int, AsyncServiceScope, object?, Task<PaginatorPage?>> generatePageCallback)
    {
        DynamicPaginatorEntry entry = new(_scopeFactory)
        {
            MessageId = 0,
            CurrentPage = 0,
            Name = Guid.NewGuid().ToString(),
            Pages = new(),
            Data = context.User
        };

        int pages = Convert.ToInt32(Math.Ceiling((decimal)elementsCount / pageSize));
        for (int i = 0; i < pages; i++)
        {
            int currentPage = i;
            var page = async Task<PaginatorPage?> (AsyncServiceScope scope, object? data) => 
                await generatePageCallback(currentPage, scope, data);

            if (page is not null)
                entry.Pages.Add(page!);
        }

        return Task.FromResult(entry);
    }

    public async Task UpdatePaginatedMessage(SocketInteraction interaction, IPaginatorEntry paginatorEntry, int page)
    {
        var messageId = await ExecutePaginator(interaction, paginatorEntry, page);
        if (messageId == 0)
            return;

        paginatorEntry.CurrentPage = (ushort)page;
        await _cacheService.UpdatePaginatorEntry(messageId, paginatorEntry);
    }

    private async Task<ulong> ExecutePaginator(SocketInteraction interaction, IPaginatorEntry entry, int page)
    {
        var embed = await entry?.GetPageContent(page)!;
        if (embed is null)
            return 0;

        var components = new ComponentBuilder()
            .WithButton("◀", "page-left")
            .WithButton("▶", "page-right");

        var result = await interaction.ModifyOriginalResponseAsync((msg) =>
        {
            msg.Embed = embed.WithFooter($"{page + 1}/{entry.GetPageCount()}").Build();
            msg.Components = components.Build();
        });

        return result.Id;
    }
}
