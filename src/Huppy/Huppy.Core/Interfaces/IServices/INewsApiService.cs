using Huppy.Core.Entities;

namespace Huppy.Core.Interfaces.IServices
{
    public interface INewsApiService
    {
        Task<NewsResponse> GetNews();
        Task PostNews();
    }
}