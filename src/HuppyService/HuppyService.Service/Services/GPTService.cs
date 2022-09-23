using Grpc.Core;
using HuppyService.Core.Constants;
using HuppyService.Core.Entities;
using HuppyService.Core.Entities.Options;
using HuppyService.Service.Protos;
using Microsoft.Extensions.Options;

namespace HuppyService.Service.Services
{
    public class GPTService : GPTProto.GPTProtoBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;
        private readonly GPTOptions _gptConfig;
        public GPTService(IHttpClientFactory clientFactory, IOptions<GPTOptions> options, ILogger<GPTService> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _gptConfig = new GPTOptions();
        }

        public override async Task<GPTOutputResponse> DavinciCompletion(GPTInputRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Prompt))
                throw new Exception("request.Prompt for GPT was empty");

            var aiContext = _gptConfig.AiContextMessage;

            if (string.IsNullOrEmpty(aiContext)) aiContext = "";
            if (!(request.Prompt.EndsWith('?') || request.Prompt.EndsWith('.'))) request.Prompt += '.';

            aiContext += request.Prompt + "\n[Huppy]:";

            GPTRequest model = new()
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
                return new GPTOutputResponse() { Answer = result!.Choices!.First()!.Text! };
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
