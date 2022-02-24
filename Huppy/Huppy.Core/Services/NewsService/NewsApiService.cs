using System.Net.Http.Json;
using Huppy.Core.Dto;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.NewsService
{
    public class NewsApiService : INewsApiService
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        public NewsApiService(ILogger<NewsApiService> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public async Task<NewsResponse> GetNews()
        {
            var fromTime = DateTime.UtcNow.AddHours(-2).ToString("o");
            var toTime = DateTime.UtcNow.ToString("o");

            var client = _clientFactory.CreateClient("News");
            var response = await client.GetAsync($"everything?q=\"war\"+\"US\"+\"Russia\"+\"ukraine\"&SortBy=relevancy&from={fromTime}&to={toTime}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content!.ReadFromJsonAsync<NewsResponse>();
                return result!;
            }
            else
            {
                var failedResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError(failedResponse);

                throw new Exception("Urban request wasn't successful");
            }
        }
    }
}