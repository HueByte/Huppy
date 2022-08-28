using Discord;
using Huppy.Core.Interfaces.IServices;

namespace Huppy.Core.Services.ScopedDataService
{
    public class ScopedDataService : IScopedDataService
    {
        public IUser? User { get; set; }
    }
}