using HuppyService.Core.Abstraction;
using HuppyService.Core.Interfaces.IRepositories;
using HuppyService.Core.Models;

namespace HuppyService.Infrastructure.Repositories;

public class TicketRepository : BaseRepository<string, Ticket, HuppyDbContext>, ITicketRepository
{
    public TicketRepository(HuppyDbContext context) : base(context) { }
}