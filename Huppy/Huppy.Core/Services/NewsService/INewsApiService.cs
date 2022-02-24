using Huppy.Core.Dto;

namespace Huppy.Core.Services.NewsService
{
    public interface INewsApiService
    {
        Task<NewsResponse> GetNews();
        Task PostNews();
    }
}