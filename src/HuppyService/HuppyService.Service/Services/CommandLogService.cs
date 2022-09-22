using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using HuppyService.Core.Interfaces.IRepositories;
using HuppyService.Core.Utilities;
using HuppyService.Service.Protos;
using Microsoft.EntityFrameworkCore;

namespace HuppyService.Service.Services
{
    public class CommandLogService : CommandLog.CommandLogBase
    {
        private readonly ICommandLogRepository _commandLogRepository;
        public CommandLogService(ICommandLogRepository commandLogRepository)
        {
            _commandLogRepository = commandLogRepository;
        }

        public override async Task<CommandLogCount> GetCount(Empty request, ServerCallContext context)
        {
            var query = await _commandLogRepository.GetAllAsync();
            var count = await query.CountAsync();

            return new CommandLogCount() { Count = count };
        }

        public override async Task<AiUsageResponse> GetAiUsage(Empty request, ServerCallContext context)
        {
            Dictionary<ulong, int> result = new();

            var query = await _commandLogRepository.GetAllAsync();
            var commandLogs = await query.ToListAsync();

            var uniqueUsers = commandLogs.GroupBy(e => e.UserId)
                                         .Select(e => e.First())
                                         .ToList();

            foreach (var user in uniqueUsers)
            {
                result.TryAdd(user.UserId, commandLogs.Where(x => x.UserId == user.UserId).Count());
            }

            var response = new AiUsageResponse();
            response.AiUsers.Add(result);

            return response;
        }

        public override async Task<AverageTimeResponse> GetAverageExecutionTime(Empty request, ServerCallContext context)
        {
            var query = await _commandLogRepository.GetAllAsync();
            var avgTime = await query.AverageAsync(e => e.ExecutionTimeMs);

            return new AverageTimeResponse() { AverageTime = avgTime };
        }

        public override async Task<CommandLogModelResponse> AddCommand(CommandLogModel request, ServerCallContext context)
        {
            Core.Models.CommandLog commandLog = new()
            {
                ChannelId = request.ChannelId,
                UserId = request.UserId,
                CommandName = request.CommandName,
                Date = Miscellaneous.UnixTimeStampToDateTime(request.UnixTime),
                ErrorMessage = request.ErrorMessage,
                ExecutionTimeMs = request.ExecutionTimeMs,
                GuildId = request.GuildId,
                IsSuccess = request.IsSuccess,
            };

            var result = await _commandLogRepository.AddAsync(commandLog);
            await _commandLogRepository.SaveChangesAsync();

            if (result)
            {
                return new CommandLogModelResponse()
                {
                    Id = commandLog.Id,
                    ChannelId = commandLog.ChannelId,
                    CommandName = commandLog.CommandName,
                    ErrorMessage = commandLog.ErrorMessage,
                    ExecutionTimeMs = commandLog.ExecutionTimeMs,
                    GuildId = (ulong)commandLog.GuildId,
                    IsSuccess = commandLog.IsSuccess,
                    UnixTime = request.UnixTime,
                    UserId = commandLog.UserId
                };
            }

            return null!;
        }

        public override async Task<CommandLogResponse> RemoveCommand(CommandLogModel request, ServerCallContext context)
        {
            Core.Models.CommandLog commandLog = new()
            {
                ChannelId = request.ChannelId,
                UserId = request.UserId,
                CommandName = request.CommandName,
                Date = Miscellaneous.UnixTimeStampToDateTime(request.UnixTime),
                ErrorMessage = request.ErrorMessage,
                ExecutionTimeMs = request.ExecutionTimeMs,
                GuildId = request.GuildId,
                IsSuccess = request.IsSuccess,
            };

            await _commandLogRepository.RemoveAsync(commandLog);
            await _commandLogRepository.SaveChangesAsync();

            return new CommandLogResponse() { IsSuccess = true };
        }
    }
}
