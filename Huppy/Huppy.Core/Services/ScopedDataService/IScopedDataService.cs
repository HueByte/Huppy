using Discord;

namespace Huppy.Core.Services.ScopedDataService
{
    public interface IScopedDataService
    {
        IUser? User { get; set; }
    }
}