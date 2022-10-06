using Google.Protobuf.WellKnownTypes;
using HuppyService.Service.Protos;
using HuppyService.Service.Protos.Models;

namespace Huppy.Core.Interfaces.IServices
{
    public interface IServerService
    {
        public Task<ServerModel> GetAsync(ulong serverId);
        public Task<ServerModel> GetOrCreateAsync(ulong serverId, string serverName, ulong defaultChannel);
        public Task<CommonResponse> UpdateAsync(ServerModel server);
        public Task<ServerModelCollection> GetAllAsync(Empty empty);
    }
}
