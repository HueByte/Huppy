using Huppy.Core.IRepositories;
using Huppy.Core.Models;

namespace Huppy.Infrastructure.Repositories
{
    public class TicketRepository : BaseRepository<string, Ticket, HuppyDbContext>, ITicketRepository
    {
        public TicketRepository(HuppyDbContext context) : base(context) { }
    }
}