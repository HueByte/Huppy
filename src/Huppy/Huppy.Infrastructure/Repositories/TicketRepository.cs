using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Models;
using Huppy.Kernel.Abstraction;

namespace Huppy.Infrastructure.Repositories
{
    public class TicketRepository : BaseRepository<string, Ticket, HuppyDbContext>, ITicketRepository
    {
        public TicketRepository(HuppyDbContext context) : base(context) { }
    }
}