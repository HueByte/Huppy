namespace Huppy.Core.Services.HuppyCacheStorage
{
    public partial class CacheStorageService
    {
        public void UpdateNextReminderFetchingDate(DateTime newTime) => _nextReminderFetchingDate = newTime;
    }
}