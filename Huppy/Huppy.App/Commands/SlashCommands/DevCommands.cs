using System.Diagnostics;
using System.Runtime;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Attributes;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.HuppyCache;
using Huppy.Core.Services.Paginator.Entities;
using Huppy.Kernel.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands.SlashCommands;

[Group("debug", "debug commands")]
[DebugCommandGroup]
[DontAutoRegister]
public class DevCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
{
    private readonly ILogger<DevCommands> _logger;
    private readonly CacheService _cacheService;
    private readonly IResourcesService _resourceService;
    private readonly DiscordShardedClient _client;
    private readonly ITicketService _ticketService;
    private readonly IPaginatorService _paginatorService;
    private const int _ticketsPerPage = 10;

    public DevCommands(ILogger<DevCommands> logger, CacheService cacheService, IResourcesService resourceService, DiscordShardedClient client,
        ITicketService ticketService, IPaginatorService paginatorService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _resourceService = resourceService;
        _client = client;
        _ticketService = ticketService;
        _paginatorService = paginatorService;
    }

    [SlashCommand("runtime", "Gets current runtime info of Huppy")]
    [RequireDev]
    public async Task GetStatus()
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Magenta)
            .WithThumbnailUrl(_client.CurrentUser.GetAvatarUrl())
            .WithDescription("Current state of resources of Huppy")
            .WithTitle("Resource Monitor");

        var cpuUsage = await _resourceService.GetCpuUsageAsync();
        var ramUsage = _resourceService.GetRamUsage();
        var shardCount = _resourceService.GetShardCount();
        var avgExecutionTime = await _resourceService.GetAverageExecutionTimeAsync();

        var upTime = _resourceService.GetUpTime();
        var upTimeFormatted = string.Format(
            @"{0}::{1}::{2}::{3}::{4}",
            upTime.Days,
            upTime.Hours,
            upTime.Minutes,
            upTime.Seconds,
            upTime.Milliseconds);

        embed.AddField("CPU", $"`{cpuUsage}`", true);
        embed.AddField("RAM", $"`{ramUsage}`", true);
        embed.AddField("Shard Count", $"`{shardCount}`", true);
        embed.AddField("Bot Uptime (DD:HH:MM:SS:MS)", $"`{upTimeFormatted}`", true);
        embed.AddField("Average command executon time", $"`{avgExecutionTime} ms`", true);
        embed.AddField("Bot Version", $"`v...`", true);
        embed.AddField("IsServerGC", $"`{GCSettings.IsServerGC}`", true);

        await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
    }

    [SlashCommand("list-tickets", "Lists all tickers")]
    [RequireDev]
    public async Task GetStatusListAsync()
    {
        var embed = new EmbedBuilder().WithColor(Color.Magenta)
            .WithThumbnailUrl(Icons.Huppy1)
            .WithTitle("Ticket list")
            .WithCurrentTimestamp();

        var ticketsCount = await _ticketService.GetCountAsync(Context.User.Id);
        if (ticketsCount < 0)
        {
            embed.WithDescription("There are no tickets ✨");
            await ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = embed.Build();
            });

            return;
        }

        var entry = await _paginatorService.GeneratePaginatorEntry(Context, ticketsCount, _ticketsPerPage, async (currentPage, scope, data) =>
        {
            var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();

            var tickets = await ticketService.GetPaginatedTickets(currentPage * _ticketsPerPage, _ticketsPerPage);

            var embed = new EmbedBuilder()
                .WithTitle("Rised tickets 📜")
                .WithColor(Color.Orange)
                .WithCurrentTimestamp();

            foreach (var ticket in tickets)
            {
                ticket.CreatedDate = DateTime.SpecifyKind(ticket.CreatedDate, DateTimeKind.Utc);

                StringBuilder sb = new();
                sb.AppendLine($"> Ticked ID:`{ticket.Id}`");
                sb.AppendLine($"> Created date: {TimestampTag.FromDateTime(ticket.CreatedDate)}");
                sb.AppendLine($"> Status: {(ticket.IsClosed ? "`Closed`" : "`Open`")}");
                sb.AppendLine($"> Topic: `{ticket.Topic}`");
                sb.AppendLine($"> Owner: `{ticket.User.Username} ({ticket.UserId})`");

                embed.AddField("📜 Ticket", sb.ToString());
            }

            return new PaginatorPage()
            {
                Embed = embed,
                Page = (ushort)currentPage
            };
        });

        await _paginatorService.SendPaginatedMessage(Context.Interaction, entry);
    }
}
