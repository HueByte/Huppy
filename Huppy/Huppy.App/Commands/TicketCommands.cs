using System.Text;
using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
using Huppy.Core.Services.PaginatorService;
using Huppy.Core.Services.PaginatorService.Entities;
using Huppy.Core.Services.TicketService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    [Group("ticket", "Give us your feedback or let me know when you see some bugs")]
    [DebugGroup]
    public class TicketCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger _logger;
        private readonly ITicketService _ticketService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPaginatorService _paginatorService;
        private const int TicketsPerPage = 5;
        public TicketCommands(ILogger<TicketCommands> logger, ITicketService ticketService, IServiceScopeFactory scopeFactory, IPaginatorService paginatorService)
        {
            _logger = logger;
            _ticketService = ticketService;
            _scopeFactory = scopeFactory;
            _paginatorService = paginatorService;
        }

        [SlashCommand("create", "Create the ticket")]
        [Ephemeral]
        public async Task CreateTicketAsync(string topic, string description)
        {
            EmbedBuilder embed;
            if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(description))
            {
                embed = new EmbedBuilder().WithColor(Color.Magenta)
                    .WithTitle("Ticket creation failed")
                    .WithDescription("You need to provide both topic and description");

                await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                return;
            }

            var ticket = await _ticketService.AddTicketAsync(Context.User, topic, description);

            if (ticket is null) throw new Exception("Ticket creation failed");

            embed = new EmbedBuilder().WithColor(Color.Magenta)
                .WithThumbnailUrl(Icons.Huppy1)
                .WithTitle("Ticket created!")
                .AddField("Ticket ID", $"`{ticket.Id}`", true)
                .AddField("Creation date", TimestampTag.FromDateTime(ticket.CreatedDate), true)
                .AddField("Status", ticket.IsClosed ? "`Closed`" : "`Open`", true)
                .AddField("Topic", ticket.Topic)
                .AddField("Ticket description", ticket.Description)
                .WithCurrentTimestamp();

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }

        [SlashCommand("close", "Delete ticket")]
        [Ephemeral]
        public async Task DeleteTicketAsync(string ticketId, string answer = "")
        {
            await _ticketService.CloseTicket(ticketId, answer);

            var embed = new EmbedBuilder().WithTitle("Ticket result")
                .WithDescription($"Ticket `{ticketId}` closed")
                .WithColor(Color.DarkMagenta);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }

        [SlashCommand("status", "Get detailed status of your ticket")]
        [Ephemeral]
        public async Task GetStatusAsync(string ticketId)
        {
            EmbedBuilder embed;

            if (string.IsNullOrEmpty(ticketId))
            {
                embed = TicketNotFound();
            }

            var ticket = await _ticketService.GetTicketAsync(ticketId, Context.User.Id);

            if (ticket is null)
            {
                embed = TicketNotFound();
            }
            else
            {
                ticket.CreatedDate = DateTime.SpecifyKind(ticket.CreatedDate, DateTimeKind.Utc);

                embed = new EmbedBuilder().WithColor(Color.Magenta)
                    .WithThumbnailUrl(Icons.Huppy1)
                    .WithTitle("Ticket details")
                    .AddField("Creation date", TimestampTag.FromDateTime(ticket.CreatedDate), true)
                    .AddField("Closed date", ticket.ClosedDate is not null ? TimestampTag.FromDateTime(ticket.CreatedDate) : "Ticket still open", true)
                    .AddField("Status", ticket.IsClosed ? "`Closed`" : "`Open`", true)
                    .AddField("Topic", ticket.Topic)
                    .AddField("Ticket description", ticket.Description)
                    .AddField("Ticket answer", !string.IsNullOrEmpty(ticket.TicketAnswer) ? ticket.TicketAnswer : "*No answer yet*")
                    .WithCurrentTimestamp();
            }

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }

        [SlashCommand("list", "Get your ticket list")]
        [Ephemeral]
        public async Task GetStatusListAsync()
        {
            var embed = new EmbedBuilder().WithColor(Color.Magenta)
                .WithThumbnailUrl(Icons.Huppy1)
                .WithTitle("Ticket list")
                .WithCurrentTimestamp();


            var ticketsCount = await _ticketService.GetCountAsync(Context.User.Id);
            if (ticketsCount < 0)
            {
                embed.WithDescription("You don't have tickets ✨");
                await ModifyOriginalResponseAsync((msg) =>
                {
                    msg.Embed = embed.Build();
                });

                return;
            }

            DynamicPaginatorEntry entry = new(_scopeFactory)
            {
                MessageId = 0,
                CurrentPage = 0,
                Name = Guid.NewGuid().ToString(),
                Pages = new(),
                Data = Context.User
            };

            int pages = Convert.ToInt32(Math.Ceiling((decimal)ticketsCount / TicketsPerPage));
            for (int i = 0; i < pages; i++)
            {
                int currentPage = i;
                var page = async Task<PaginatorPage?> (AsyncServiceScope scope, object? data) =>
                {
                    if (data is not IUser user) return null;

                    var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();

                    var tickets = await ticketService.GetPaginatedTickets(user.Id, currentPage * TicketsPerPage, TicketsPerPage);

                    var embed = new EmbedBuilder()
                        .WithTitle("Your tickets 📜")
                        .WithDescription("To see detailed status of your ticked use `/ticket status` with copied ID")
                        .WithThumbnailUrl(user.GetAvatarUrl())
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

                        embed.AddField("📜 Ticket", sb.ToString());
                    }

                    return new PaginatorPage()
                    {
                        Embed = embed,
                        Page = (ushort)currentPage
                    };
                };

                if (page is not null) entry.Pages.Add(page!);
            }

            await _paginatorService.SendPaginatedMessage(Context.Interaction, entry);
        }

        private static EmbedBuilder TicketNotFound()
        {
            return new EmbedBuilder().WithTitle("Ticket not found")
                .WithDescription("Please check provided ticket ID and try again")
                .WithColor(Color.Orange)
                .WithCurrentTimestamp();
        }
    }
}