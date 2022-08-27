using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.ActivityService
{
    public interface IActivityControlService
    {
        Task Initialize();
        Task ChangeActivity(AsyncServiceScope scope);
        Task<IActivity?> GetActivity(AsyncServiceScope scope);
    }
}