using Huppy.Core.Entities;

namespace Huppy.Core.Services.HuppyCacheService
{
    public partial class CacheService
    {
        private int _maxCacheMessageSize;

        public void SetMaxMessageCacheSize(int size) => _maxCacheMessageSize = size;
        public Task AddPaginatedMessage(ulong messageId, PaginatedMessage message)
        {
            if (PaginatedMessages.Count == _maxCacheMessageSize)
                PaginatedMessages.RemoveAt(PaginatedMessages.Count - 1);

            PaginatedMessages.Insert(0, messageId, message);

            return Task.CompletedTask;
        }

        public Task<PaginatedMessage?> GetPaginatedMessage(ulong key)
        {
            var result = PaginatedMessages[key] as PaginatedMessage;
            return Task.FromResult(result);
        }
    }
}