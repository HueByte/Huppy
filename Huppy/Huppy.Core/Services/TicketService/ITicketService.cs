using Discord;
using Huppy.Core.Models;

namespace Huppy.Core.Services.TicketService
{
    public interface ITicketService
    {
        Task<int> GetCountAsync(ulong userId);
        Task<IEnumerable<Ticket>> GetTicketsAsync();
        Task<IEnumerable<Ticket>> GetTicketsAsync(ulong userId);
        Task<IEnumerable<Ticket>> GetPaginatedTickets(ulong userId, int skip, int take);
        Task<Ticket?> GetTicketAsync(string ticketId, ulong userId);
        Task<Ticket?> AddTicketAsync(IUser user, string topic, string description);
        Task RemoveTicketAsync(string ticketId);
        Task UpdateTicketAsync(string ticketId, string description);
        Task CloseTicket(string ticketId, string answer);
    }
}