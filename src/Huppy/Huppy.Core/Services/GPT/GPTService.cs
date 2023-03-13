using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Huppy.Core.Entities;
using Huppy.Core.Interfaces.IServices;
using Huppy.Kernel;
using Huppy.Kernel.Constants;
using HuppyService.Service.Protos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.GPT
{
    public class GPTService : IGPTService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        private readonly HuppyService.Service.Protos.GPTProto.GPTProtoClient _gptClient;
        public GPTService(HuppyService.Service.Protos.GPTProto.GPTProtoClient gptClient, IHttpClientFactory clientFactory, ILogger<GPTService> logger, AppSettings settings)
        {
            _gptClient = gptClient;
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
            var result = await _gptClient.DavinciCompletionAsync(new GPTInputRequest() { Prompt = prompt });
            return result.Answer;
        }
    }
}