using Discord;
using Huppy.Core.Models;
using HuppyService.Service.Protos.Models;

namespace Huppy.Core.Interfaces.IServices
{
    public interface ITicketService
    {
        Task<int> GetCountAsync(ulong userId);
        Task<IList<TicketModel>> GetTicketsAsync();
        Task<IList<TicketModel>> GetTicketsAsync(ulong userId);
        Task<IList<TicketModel>> GetPaginatedTickets(int skip, int take);
        Task<IList<TicketModel>> GetPaginatedTickets(ulong userId, int skip, int take);
        Task<TicketModel?> GetTicketAsync(string ticketId, ulong userId);
        Task<TicketModel?> AddTicketAsync(IUser user, string topic, string description);
        Task RemoveTicketAsync(string ticketId);
        Task UpdateTicketAsync(string ticketId, string description);
        Task CloseTicket(string ticketId, string answer);
    }
}