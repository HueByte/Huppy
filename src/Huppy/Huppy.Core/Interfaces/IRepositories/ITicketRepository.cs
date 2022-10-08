using Huppy.Core.Models;
using Huppy.Kernel;

namespace Huppy.Core.Interfaces.IRepositories
{
    [Obsolete]
    public interface ITicketRepository : IRepository<string, Ticket>
    {

    }
}