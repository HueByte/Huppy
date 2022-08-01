using System.Diagnostics;
using Huppy.Core.Entities;
using Huppy.Core.Services.PaginatorService.Entities;

namespace Huppy.Core.Services.HuppyCacheService
{
    public partial class CacheService
    {
        private int _maxCacheMessageSize;

        public void SetMaxMessageCacheSize(int size) => _maxCacheMessageSize = size;
        public Task AddPaginatedEntry(ulong messageId, IPaginatorEntry entry)
        {
            if (entry is null)
                throw new Exception("Paginator entry cannot be null");

            if (PaginatorEntries.Count >= _maxCacheMessageSize)
                PaginatorEntries.RemoveAt(PaginatorEntries.Count - 1);

            PaginatorEntries.Insert(0, messageId, entry);

            return Task.CompletedTask;
        }

        public Task UpdatePaginatorEntry(ulong messageId, IPaginatorEntry entry)
        {
            if (PaginatorEntries.Contains(messageId))
                PaginatorEntries[(object)messageId] = entry;

            return Task.CompletedTask;
        }

        public Task<IPaginatorEntry> GetPaginatorEntry(ulong messageId)
        {
            var result = (IPaginatorEntry)PaginatorEntries[(object)messageId]!;
            return Task.FromResult(result);
        }
    }
}