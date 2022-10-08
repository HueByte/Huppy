using Discord;
using Huppy.Core.Models;
using HuppyService.Service.Protos.Models;
using Microsoft.EntityFrameworkCore.Internal;

namespace Huppy.Core.Interfaces.IServices
{
    public interface IReminderService
    {
        TimeSpan FetchReminderFrequency { get; }
        Task RegisterFreshRemindersAsync();
        Task<int> GetRemindersCount(ulong userId);
        Task<IList<ReminderModel>> GetSortedUserReminders(ulong userId, int skip, int take);
        Task<ReminderModel> GetReminderAsync(ulong userId, int reminderId);
        Task<IList<ReminderModel>> GetUserRemindersAsync(ulong userId);
        Task AddReminderAsync(DateTime date, ulong userId, string message);
        Task AddReminderAsync(DateTime date, IUser user, string message);
        Task RemoveReminderAsync(ReminderModel reminder);
        Task RemoveReminderRangeAsync(string[] ids);
        Task RemoveReminderRangeAsync(int[] ids);
    }
}