using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Huppy.Core.Interfaces.IServices;
using HuppyService.Service.Protos;

namespace Huppy.Core.Services.CommandLog
{
    public class CommandLogService : ICommandLogService
    {
        private readonly HuppyService.Service.Protos.CommandLog.CommandLogClient _commandLogClient;
        public CommandLogService(HuppyService.Service.Protos.CommandLog.CommandLogClient commandLogClient)
        {
            _commandLogClient = commandLogClient;
        }

        public async Task<CommandLogModelResponse> AddCommand(CommandLogModel commandLog)
        {
            var result = await _commandLogClient.AddCommandAsync(commandLog);
            
            return result;
        }

        public async Task<MapField<ulong, AiUser>> GetAiUsage()
        {
            // implement single instance of empty?
            var result = await _commandLogClient.GetAiUsageAsync(new Empty());
            
            return result.AiUsers;
        }

        public async Task<double> GetAverageExecutionTime()
        {
            var result = await _commandLogClient.GetAverageExecutionTimeAsync(new Empty());
            
            return result.AverageTime;
        }

        public async Task<int> GetCount()
        {
            var result = await _commandLogClient.GetCountAsync(new Empty());
            
            return result.Count;
        }

        public async Task<bool> RemoveCommand(CommandLogModel commandLog)
        {
            var result = await _commandLogClient.RemoveCommandAsync(commandLog);

            return result.IsSuccess;
        }
    }
}
