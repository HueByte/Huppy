using Huppy.Core.Entities;

namespace Huppy.Core.Services.PaginatedEmbedService
{
    public interface IPaginatorEmbedService
    {
        Task Initialize();
        List<PaginatorEntry> GetPaginatorEntries();
    }
}