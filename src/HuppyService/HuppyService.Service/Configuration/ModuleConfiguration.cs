using HuppyService.Core.Interfaces.IRepositories;
using HuppyService.Infrastructure;
using HuppyService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

namespace HuppyService.Service.Configuration
{
    public class ModuleConfiguration
    {
        private readonly IServiceCollection _services;
        private readonly IConfiguration _config;
        public ModuleConfiguration(IConfiguration config, IServiceCollection? services = null)
        {
            _services = services ?? new ServiceCollection();
            _config = config;
        }

        public ModuleConfiguration AddAppConfigurator()
        {
            _services.AddSingleton<AppConfigurator>();

            return this;
        }

        public ModuleConfiguration AddGrpc()
        {
            _services.AddGrpc();
            return this;
        }

        public ModuleConfiguration AddDatabase()
        {
            _services.AddDbContext(_config["MiscellaneousOptions:ConnectionString"]);
            return this;
        }

        public ModuleConfiguration AddRepositories()
        {
            _services.AddScoped<IUserRepository, UserRepository>();
            _services.AddScoped<IServerRepository, ServerRepository>();
            _services.AddScoped<ICommandLogRepository, CommandLogRepository>();
            _services.AddScoped<IReminderRepository, ReminderRepository>();
            _services.AddScoped<ITicketRepository, TicketRepository>();

            return this;
        }

        public ModuleConfiguration AddHttpClients()
        {
            _services.AddHttpClient("GPT", httpclient =>
            {
                httpclient.BaseAddress = new Uri(_config["GPTOptions:BaseUrl"]);
                httpclient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config["GPTOptions:ApiKey"]}");

                if (!string.IsNullOrEmpty(_config["GPTOptions:Organization"]))
                    httpclient.DefaultRequestHeaders.Add("OpenAI-Organization", _config["GPTOptions:Organization"]);
            });

            _services.AddHttpClient("Urban", httpClient =>
            {
                httpClient.BaseAddress = new Uri(_config["UrbanApioptions:BaseUrl"]);
                httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", _config["UrbanApioptions:Host"]);
                httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _config["UrbanApioptions:Key"]);
            });

            return this;
        }
    }
}
