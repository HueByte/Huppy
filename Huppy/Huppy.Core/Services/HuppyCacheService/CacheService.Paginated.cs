using Huppy.Core.Entities;

namespace Huppy.Core.Services.HuppyCacheService
{
    public partial class CacheService
    {
        private int _maxCacheMessageSize;

        public void SetMaxMessageCacheSize(int size) => _maxCacheMessageSize = size;
        public Task AddPaginatedMessage(ulong messageId, PaginatedMessageState message)
        {
            if (message is null)
                throw new Exception("Paginated message was empty");

            if (PaginatedMessages.Count == _maxCacheMessageSize)
                PaginatedMessages.RemoveAt(PaginatedMessages.Count - 1);

            PaginatedMessages.Insert(0, messageId, message);

            return Task.CompletedTask;
        }

        public Task UpdatePaginatedMessage(ulong key, PaginatedMessageState message)
        {
            if (PaginatedMessages.Contains(key))
                PaginatedMessages[(object)key] = message;

            return Task.CompletedTask;
        }

        public Task<PaginatedMessageState?> GetPaginatedMessage(ulong key)
        {
            var result = (PaginatedMessageState?)PaginatedMessages[(object)key];
            return Task.FromResult(result);
        }

        public Task<PaginatedMessageState?> GetPaginatedMessage(int index)
        {
            var result = PaginatedMessages[index] as PaginatedMessageState;
            return Task.FromResult(result);
        }
    }
}