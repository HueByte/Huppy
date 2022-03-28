using Huppy.Core.Entities;

namespace Huppy.Core.Services.HuppyCacheService
{
    public partial class CacheService
    {
        private int _maxCacheMessageSize;

        public void SetMaxMessageCacheSize(int size) => _maxCacheMessageSize = size;
        public Task AddPaginatedMessage(ulong messageId, PaginatedMessage message)
        {
            if (message is null)
                throw new Exception("Paginated message was empty");

            if (PaginatedMessages.Count == _maxCacheMessageSize)
                PaginatedMessages.RemoveAt(PaginatedMessages.Count - 1);

            PaginatedMessages.Insert(0, messageId, message);

            return Task.CompletedTask;
        }

        public Task UpdatePaginatedMessage(ulong key, PaginatedMessage message)
        {
            if (PaginatedMessages.Contains(key))
                PaginatedMessages[(object)key] = message;

            return Task.CompletedTask;
        }

        public Task<PaginatedMessage?> GetPaginatedMessage(ulong key)
        {
            var result = (PaginatedMessage?)PaginatedMessages[(object)key];
            return Task.FromResult(result);
        }

        public Task<PaginatedMessage?> GetPaginatedMessage(int index)
        {
            var result = PaginatedMessages[index] as PaginatedMessage;
            return Task.FromResult(result);
        }
    }
}