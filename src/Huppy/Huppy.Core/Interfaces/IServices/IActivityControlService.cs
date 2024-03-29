using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Interfaces.IServices
{
    public interface IActivityControlService
    {
        TimeSpan UpdateStatusFrequency { get; }
        Task ChangeActivity(AsyncServiceScope scope);
        Task<IActivity?> GetActivity(AsyncServiceScope scope);
    }
}