using Discord;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Huppy.Core.Services.Ticket;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    public TicketService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<int> GetCountAsync(ulong userId)
    {
        // EFC lazy loading, DB is not called here
        var tickets = await _ticketRepository.GetAllAsync();

        // modify database query
        return await tickets.Where(ticket => ticket.UserId == userId).CountAsync();
    }

    public async Task<IEnumerable<Models.Ticket>> GetTicketsAsync()
    {
        return await _ticketRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Models.Ticket>> GetTicketsAsync(ulong userId)
    {
        var tickets = await _ticketRepository.GetAllAsync();
        return tickets.Where(ticket => ticket.UserId == userId);
    }

    public async Task<IEnumerable<Models.Ticket>> GetPaginatedTickets(ulong userId, int skip, int take)
    {
        var tickets = (await _ticketRepository.GetAllAsync())
            .Where(ticket => ticket.UserId == userId)
            .OrderBy(ticket => ticket.IsClosed)
            .ThenByDescending(ticket => ticket.CreatedDate)
            .Skip(skip)
            .Take(take)
            .ToList();

        return tickets;
    }

    public async Task<Models.Ticket?> GetTicketAsync(string ticketId, ulong userId)
    {
        var tickets = await _ticketRepository.GetAllAsync();

        return await tickets.FirstOrDefaultAsync(ticket => ticket.Id == ticketId && ticket.UserId == userId);
    }

    public async Task<Models.Ticket?> AddTicketAsync(IUser user, string topic, string description)
    {
        if (string.IsNullOrEmpty(description)) throw new ArgumentException("Ticket description cannot be null");

        Models.Ticket ticket = new()
        {
            Id = Guid.NewGuid().ToString(),
            Topic = topic,
            Description = description,
            CreatedDate = DateTime.UtcNow,
            TicketAnswer = null,
            ClosedDate = null,
            IsClosed = false,
            UserId = user.Id,
        };

        await _ticketRepository.AddAsync(ticket);
        await _ticketRepository.SaveChangesAsync();

        return ticket;
    }

    public async Task RemoveTicketAsync(string ticketId)
    {
        if (string.IsNullOrEmpty(ticketId)) throw new ArgumentException("Ticket cannot be null or empty");

        await _ticketRepository.RemoveAsync(ticketId);
        await _ticketRepository.SaveChangesAsync();
    }

    public async Task UpdateTicketAsync(string ticketId, string description)
    {
        if (string.IsNullOrEmpty(ticketId) || string.IsNullOrEmpty(description))
            throw new ArgumentException("Both ticked ID and ticket description cannot be null or empty");

        var ticket = await _ticketRepository.GetAsync(ticketId);

        if (ticket is null) throw new Exception("Ticket doesn't exist");

        ticket.Description = description;

        await _ticketRepository.UpdateAsync(ticket);
        await _ticketRepository.SaveChangesAsync();
    }

    public async Task CloseTicket(string ticketId, string answer)
    {
        if (string.IsNullOrEmpty(ticketId))
            throw new ArgumentException("Ticked ID cannot be empty");

        var ticket = await _ticketRepository.GetAsync(ticketId);

        if (ticket is null) throw new Exception("Ticket doesn't exist");

        ticket.IsClosed = true;
        ticket.TicketAnswer = answer;
        ticket.ClosedDate = DateTime.UtcNow;

        await _ticketRepository.UpdateAsync(ticket);
        await _ticketRepository.SaveChangesAsync();
    }
}
