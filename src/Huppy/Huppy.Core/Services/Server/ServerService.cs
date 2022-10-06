using Google.Protobuf.WellKnownTypes;
using Huppy.Core.Interfaces.IServices;
using HuppyService.Service.Protos;
using HuppyService.Service.Protos.Models;

namespace Huppy.Core.Services.Server
{
    public class ServerService : IServerService
    {
        private readonly ServerProto.ServerProtoClient _serverClient;
        public ServerService(ServerProto.ServerProtoClient serverClient)
        {
            _serverClient = serverClient;
        }

        public async Task<ServerModelCollection> GetAllAsync(Empty empty)
        {
            var result = await _serverClient.GetAllAsync(new Empty());
            return result;
        }

        public async Task<ServerModel> GetAsync(ulong serverId)
        {
            var result = await _serverClient.GetAsync(new ServerIdInput() { Id = serverId });
            return result;
        }

        public async Task<ServerModel> GetOrCreateAsync(ulong serverId, string serverName, ulong defaultChannel)
        {
            var result = await _serverClient.GetOrCreateAsync(new GetOrCreateServerInput()
            {
                Id = serverId,
                ServerName = serverName,
                DefaultChannel = defaultChannel
            });

            return result;
        }

        public async Task<CommonResponse> UpdateAsync(ServerModel server)
        {
            var updateResult = await _serverClient.UpdateAsync(server);
            
            return updateResult;
        }
    }
}
