using System.Reflection.Metadata.Ecma335;
using Discord;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Models;
using HuppyService.Service.Protos;
using HuppyService.Service.Protos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Huppy.Core.Services.Ticket;

public class TicketService : ITicketService
{
    // private readonly ITicketRepository _ticketRepository;
    private readonly TicketProto.TicketProtoClient _ticketClient;
    public TicketService(TicketProto.TicketProtoClient ticketClient, ITicketRepository ticketRepository)
    {
        // _ticketRepository = ticketRepository;
        _ticketClient = ticketClient;
    }

    public async Task<int> GetCountAsync(ulong userId)
    {
        var result = await _ticketClient.GetCountAsync(new() { Id = userId });

        return result.Number;
    }

    public async Task<IList<TicketModel>> GetTicketsAsync()
    {
        var result = await _ticketClient.GetTicketsAsync(new());
        return result.TicketsModels;
    }

    public async Task<IList<TicketModel>> GetTicketsAsync(ulong userId)
    {
        var tickets = await _ticketClient.GetUserTicketsAsync(new() { Id = userId });
        return tickets.TicketsModels;
    }

    public async Task<IList<TicketModel>> GetPaginatedTickets(int skip, int take)
    {
        var tickets = await _ticketClient.GetPaginatedTicketsAsync(new() { Skip = skip, Take = take });

        return tickets.TicketsModels;
    }

    public async Task<IList<TicketModel>> GetPaginatedTickets(ulong userId, int skip, int take)
    {
        var tickets = await _ticketClient.GetUserPaginatedTicketsAsync(new() { Skip = skip, Take = take, UserId = userId });

        return tickets.TicketsModels;
    }

    public async Task<TicketModel?> GetTicketAsync(string ticketId, ulong userId)
    {
        var ticket = await _ticketClient.GetTicketAsync(new() { TicketId = ticketId, UserId = userId });
        // var tickets = await _ticketRepository.GetAllAsync();
        return ticket;

        // return await tickets.FirstOrDefaultAsync(ticket => ticket.Id == ticketId && ticket.UserId == userId);
    }

    public async Task<TicketModel?> AddTicketAsync(IUser user, string topic, string description)
    {
        if (string.IsNullOrEmpty(description)) throw new ArgumentException("Ticket description cannot be null");

        var result = await _ticketClient.AddTicketAsync(new() { UserId = user.Id, Description = description, Topic = topic });
        return result;

        // Models.Ticket ticket = new()
        // {
        //     Id = Guid.NewGuid().ToString(),
        //     Topic = topic,
        //     Description = description,
        //     CreatedDate = DateTime.UtcNow,
        //     TicketAnswer = null,
        //     ClosedDate = null,
        //     IsClosed = false,
        //     UserId = user.Id,
        // };

        // await _ticketRepository.AddAsync(ticket);
        // await _ticketRepository.SaveChangesAsync();

        // return ticket;
    }

    public async Task RemoveTicketAsync(string ticketId)
    {
        if (string.IsNullOrEmpty(ticketId)) throw new ArgumentException("Ticket cannot be null or empty");

        var result = await _ticketClient.RemoveTicketAsync(new() { Id = ticketId });

        // await _ticketRepository.RemoveAsync(ticketId);
        // await _ticketRepository.SaveChangesAsync();
    }

    public async Task UpdateTicketAsync(string ticketId, string description)
    {
        if (string.IsNullOrEmpty(ticketId) || string.IsNullOrEmpty(description))
            throw new ArgumentException("Both ticked ID and ticket description cannot be null or empty");

        var result = await _ticketClient.UpdateTicketAsync(new() { TicketId = ticketId, Description = description });


        // var ticket = await _ticketRepository.GetAsync(ticketId);

        // if (ticket is null) throw new Exception("Ticket doesn't exist");

        // ticket.Description = description;

        // await _ticketRepository.UpdateAsync(ticket);
        // await _ticketRepository.SaveChangesAsync();
    }

    public async Task CloseTicket(string ticketId, string answer)
    {
        if (string.IsNullOrEmpty(ticketId))
            throw new ArgumentException("Ticked ID cannot be empty");

        _ = await _ticketClient.CloseTicketAsync(new() { TicketId = ticketId, Answer = answer });

        // var ticket = await _ticketRepository.GetAsync(ticketId);

        // if (ticket is null) throw new Exception("Ticket doesn't exist");

        // ticket.IsClosed = true;
        // ticket.TicketAnswer = answer;
        // ticket.ClosedDate = DateTime.UtcNow;

        // await _ticketRepository.UpdateAsync(ticket);
        // await _ticketRepository.SaveChangesAsync();
    }
}
