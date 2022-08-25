using System.Net.Http.Json;
using Huppy.Core.Common.Constants;
using Huppy.Core.Dto;
using Huppy.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.GPTService
{
    public class GPTService : IGPTService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        public GPTService(IHttpClientFactory clientFactory, ILogger<GPTService> logger, AppSettings settings)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _settings = settings;
        }

        public async Task GetEngines()
        {
            var client = _clientFactory.CreateClient("GPT");
            var result = await client.GetAsync("https://api.openai.com/v1/engines");

            _logger.LogInformation("{response}", await result.Content.ReadAsStringAsync());
        }

        public async Task<string> DavinciCompletion(string prompt)
        {
            if (String.IsNullOrEmpty(prompt))
                throw new Exception("Prompt for GPT was empty");

            var aiContext = _settings?.GPT?.AiContextMessage;

            if (string.IsNullOrEmpty(aiContext)) aiContext = "";
            if (!(prompt.EndsWith('?') || prompt.EndsWith('.'))) prompt += '.';

            aiContext += prompt + "\n[Huppy]:";

            GPTDto model = new()
            {
                MaxTokens = 200,
                Prompt = aiContext,
                Temperature = 0.6,
                FrequencyPenalty = 0.5,
                N = 1
            };

            var client = _clientFactory.CreateClient("GPT");

            var response = await client.PostAsJsonAsync(GPTEndpoints.TextDavinciCompletions, model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content!.ReadFromJsonAsync<GPTResponse>();
                return result!.Choices!.First()!.Text!;
            }
            else
            {
                var failedResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError("{response}", failedResponse);

                throw new Exception("GPT request wasn't successful");
            }
        }
    }
}