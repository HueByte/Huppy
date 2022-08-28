using System.Net.Http.Json;
using Huppy.Core.Entities;
using Huppy.Core.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.UrbanService
{
    public class UrbanService : IUrbanService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;
        public UrbanService(ILogger<UrbanService> logger, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<UrbanResponse> GetDefinition(string term)
        {
            if (string.IsNullOrEmpty(term))
                throw new Exception("Term for Urban was empty");

            var client = _clientFactory.CreateClient("Urban");
            var response = await client.GetAsync($"?term={term}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content!.ReadFromJsonAsync<UrbanResponse>();
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