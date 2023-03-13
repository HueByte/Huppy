using System.Security.Authentication.ExtendedProtection;
using Google.Protobuf.WellKnownTypes;
using Huppy.Core.Interfaces.IServices;
using HuppyService.Service;
using HuppyService.Service.Protos.Models;

namespace Huppy.Core.Services.CommandLog
{
    public class CommandLogService : ICommandLogService
    {
        private readonly HuppyService.Service.Protos.CommandLogProto.CommandLogProtoClient _commandLogClient;
        public CommandLogService(HuppyService.Service.Protos.CommandLogProto.CommandLogProtoClient commandLogClient)
        {
            _commandLogClient = commandLogClient;
        }

        public async Task<CommandLogModel> AddCommand(CommandLogModel commandLog)
        {
            var result = await _commandLogClient.AddCommandAsync(commandLog);

            return result;
        }

        public async Task<IDictionary<ulong, int>> GetAiUsage()
        {
            // implement single instance of empty?
            var result = await _commandLogClient.GetAiUsageAsync(new HuppyService.Service.Protos.Void());

            return result.AiUsers;
        }

        public async Task<double> GetAverageExecutionTime()
        {
            var result = await _commandLogClient.GetAverageExecutionTimeAsync(new HuppyService.Service.Protos.Void());

            return result.AverageTime;
        }

        public async Task<int> GetCount()
        {
            var result = await _commandLogClient.GetCountAsync(new HuppyService.Service.Protos.Void());

            return result.Number;
        }

        public async Task<bool> RemoveCommand(CommandLogModel commandLog)
        {
            var result = await _commandLogClient.RemoveCommandAsync(commandLog);

            return result.IsSuccess;
        }
    }
}
