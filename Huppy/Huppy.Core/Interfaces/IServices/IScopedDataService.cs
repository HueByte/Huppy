using Discord;

namespace Huppy.Core.Interfaces.IServices
{
    public interface IScopedDataService
    {
        IUser? User { get; set; }
    }
}