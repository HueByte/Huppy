using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using HuppyService.Core.Interfaces.IRepositories;
using HuppyService.Core.Models;
using HuppyService.Service.Protos;
using HuppyService.Service.Protos.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace HuppyService.Service.Services
{
    public class ServerService : ServerProto.ServerProtoBase
    {
        private readonly IServerRepository _serverRepository;
        public ServerService(IServerRepository serverRepository)
        {
            _serverRepository = serverRepository;
        }

        public override async Task<ServerModel> Get(ServerIdInput request, ServerCallContext context)
        {
            var server = await _serverRepository.GetAsync(request.Id);
            
            return new ServerModel()
            {
                Id = server.Id,
                GreetMessage = server.GreetMessage,
                RoleId = server.RoleID,
                ServerName = server.ServerName,
                UseGreet = server.UseGreet,
                Rooms = new()
                {
                    GreetingRoom = server.Rooms.GreetingRoom,
                    OutputRoom = server.Rooms.OutputRoom,
                    ServerId = server.Id
                }
            };
        }

        public override async Task<ServerModelCollection> GetAll(Empty request, ServerCallContext context)
        {
            var query = await _serverRepository.GetAllAsync();
            var servers = await query.Include(e => e.Rooms).ToListAsync();

            ServerModelCollection result = new();
            result.ServerModels.AddRange(servers.Select(server => new ServerModel()
            {
                Id = server.Id,
                ServerName = server.ServerName,
                UseGreet = server.UseGreet,
                GreetMessage = server.GreetMessage,
                RoleId = server.RoleID,
                ServerRoomsId = server.ServerRoomsId,
                Rooms = new()
                {
                    Id = server.Rooms.Id,
                    GreetingRoom = server.Rooms.GreetingRoom,
                    OutputRoom = server.Rooms.OutputRoom,
                    ServerId = server.Rooms.ServerId
                }
            }).ToList());

            return result;
        }

        public override async Task<ServerModel> GetOrCreate(GetOrCreateServerInput request, ServerCallContext context)
        {
            var server = await _serverRepository.GetAsync(request.Id);

            if (server is not null)
            {
                if (server.Rooms is null)
                {
                    server.Rooms = new()
                    {
                        OutputRoom = request.DefaultChannel,
                        GreetingRoom = default,
                        ServerId = request.Id
                    };

                    await _serverRepository.UpdateAsync(server);
                    await _serverRepository.SaveChangesAsync();
                }

                return new ServerModel()
                {
                    Id = server.Id,
                    GreetMessage = server.GreetMessage,
                    RoleId = server.RoleID,
                    ServerName = server.ServerName,
                    UseGreet = server.UseGreet,
                    Rooms = new()
                    {
                        GreetingRoom = server.Rooms.GreetingRoom,
                        OutputRoom = server.Rooms.OutputRoom,
                        ServerId = server.Id
                    }
                };
            }

            server = new()
            {
                Id = request.Id,
                GreetMessage = "Welcome {username}!",
                ServerName = request.ServerName,
                RoleID = 0,
                UseGreet = false,
                Rooms = new()
                {
                    OutputRoom = request.DefaultChannel,
                    GreetingRoom = 0,
                    ServerId = request.Id
                }
            };

            await _serverRepository.AddAsync(server);
            await _serverRepository.SaveChangesAsync();

            return new ServerModel()
            {
                Id = server.Id,
                GreetMessage = server.GreetMessage,
                RoleId = server.RoleID,
                ServerName = server.ServerName,
                UseGreet = server.UseGreet,
                Rooms = new()
                {
                    GreetingRoom = server.Rooms.GreetingRoom,
                    OutputRoom = server.Rooms.OutputRoom,
                    ServerId = server.Id
                }
            };
        }

        public override async Task<CommonResponse> Update(ServerModel request, ServerCallContext context)
        {
            Server server = new()
            {
                Id = request.Id,
                ServerName = request.ServerName,
                GreetMessage = request.GreetMessage,
                RoleID = request.RoleId,
                UseGreet = request.UseGreet,
                ServerRoomsId = request.ServerRoomsId,
                Rooms = new() {
                    Id = request.Id,
                    GreetingRoom = request.Rooms.GreetingRoom,
                    OutputRoom = request.Rooms.OutputRoom,
                    ServerId = request.Rooms.ServerId
                }
            };

            await _serverRepository.UpdateAsync(server);
            return new CommonResponse() { IsSuccess = true };
        }
    }
}
