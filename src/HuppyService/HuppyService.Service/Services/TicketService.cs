using Grpc.Core;
using HuppyService.Core.Interfaces.IRepositories;
using HuppyService.Service.Protos;
using HuppyService.Service.Protos.Models;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HuppyService.Service.Services
{
    public class TicketService : TicketProto.TicketProtoBase
    {
        private readonly ITicketRepository _ticketRepository;
        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public override Task<TicketModel> AddTicket(AddTicketInput request, ServerCallContext context)
        {
            return base.AddTicket(request, context);
        }

        public override Task<CommonResponse> CloseTicket(CloseTicketInput request, ServerCallContext context)
        {
            return base.CloseTicket(request, context);
        }

        public override Task<Protos.Int32> GetCountAsync(UserId request, ServerCallContext context)
        {
            return base.GetCountAsync(request, context);
        }

        public override Task<TicketModelCollection> GetPaginatedTickets(GetPaginatedTicketsInput request, ServerCallContext context)
        {
            return base.GetPaginatedTickets(request, context);
        }

        public override Task<TicketModel> GetTicket(GetTicketInput request, ServerCallContext context)
        {
            return base.GetTicket(request, context);
        }

        public override Task<TicketModelCollection> GetTickets(Protos.Void request, ServerCallContext context)
        {
            return base.GetTickets(request, context);
        }

        public override Task<TicketModelCollection> GetUserPaginatedTickets(GetUserPaginatedTicketsInput request, ServerCallContext context)
        {
            return base.GetUserPaginatedTickets(request, context);
        }

        public override Task<TicketModelCollection> GetUserTickets(UserId request, ServerCallContext context)
        {
            return base.GetUserTickets(request, context);
        }

        public override Task<CommonResponse> RemoveTicketAsync(StringId request, ServerCallContext context)
        {
            return base.RemoveTicketAsync(request, context);
        }

        public override Task<CommonResponse> UpdateTicketAsync(TicketUpdateInput request, ServerCallContext context)
        {
            return base.UpdateTicketAsync(request, context);
        }
    }
}
