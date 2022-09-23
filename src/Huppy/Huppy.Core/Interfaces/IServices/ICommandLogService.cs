using Google.Protobuf.Collections;
using HuppyService.Service.Protos;

namespace Huppy.Core.Interfaces.IServices
{
    public interface ICommandLogService
    {
        Task<int> GetCount();
        Task<double> GetAverageExecutionTime();
        Task<MapField<ulong, int>> GetAiUsage();
        Task<CommandLogModel> AddCommand(CommandLogModel model);
        Task<bool> RemoveCommand(CommandLogModel model);
    }
}
