using Google.Protobuf;
using Grpc.Core;
using HuppyService.Core.Interfaces.IRepositories;
using HuppyService.Core.Models;
using HuppyService.Core.Utilities;
using HuppyService.Service.Protos;
using HuppyService.Service.Protos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Net.Sockets;

namespace HuppyService.Service.Services
{
    public class TicketService : TicketProto.TicketProtoBase
    {
        private readonly ITicketRepository _ticketRepository;
        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public override async Task<TicketModel> AddTicket(AddTicketInput request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Description)) throw new ArgumentException("Ticket description cannot be null");

            Ticket ticket = new()
            {
                Id = Guid.NewGuid().ToString(),
                Topic = request.Topic,
                Description = request.Description,
                CreatedDate = DateTime.UtcNow,
                TicketAnswer = null,
                ClosedDate = null,
                IsClosed = false,
                UserId = request.UserId,
            };

            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return new TicketModel()
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                Description = ticket.Description,
                Topic = ticket.Topic,
                IsClosed = ticket.IsClosed,
                TicketAnswer = ticket.TicketAnswer,
                ClosedDate = ticket.ClosedDate is null ? 0 : Miscellaneous.DateTimeToUnixTimeStamp((DateTime)ticket.ClosedDate),
                CreatedDate = Miscellaneous.DateTimeToUnixTimeStamp(ticket.CreatedDate)
            };
        }

        public override async Task<CommonResponse> CloseTicket(CloseTicketInput request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.TicketId))
                throw new ArgumentException("Ticked ID cannot be empty");

            var ticket = await _ticketRepository.GetAsync(request.TicketId);

            if (ticket is null) throw new Exception("Ticket doesn't exist");

            ticket.IsClosed = true;
            ticket.TicketAnswer = request.Answer;
            ticket.ClosedDate = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return new CommonResponse() { IsSuccess = true };
        }

        public override async Task<Protos.Int32> GetCountAsync(UserId request, ServerCallContext context)
        {
            var tickets = await _ticketRepository.GetAllAsync();

            // modify database query
            var result = await tickets.Where(ticket => ticket.UserId == request.Id).CountAsync();
            return new Protos.Int32() { Number = result };
        }

        public override async Task<TicketModelCollection> GetPaginatedTickets(GetPaginatedTicketsInput request, ServerCallContext context)
        {
            var tickets = (await _ticketRepository.GetAllAsync())
                .OrderBy(ticket => ticket.IsClosed)
                .ThenByDescending(ticket => ticket.CreatedDate)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToList();

            var result = new TicketModelCollection();
            result.TicketsModels.AddRange(tickets.Select(ticket => new TicketModel
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                Description = ticket.Description,
                Topic = ticket.Topic,
                IsClosed = ticket.IsClosed,
                TicketAnswer = ticket.TicketAnswer,
                ClosedDate = ticket.ClosedDate is null ? 0 : Miscellaneous.DateTimeToUnixTimeStamp((DateTime)ticket.ClosedDate),
                CreatedDate = Miscellaneous.DateTimeToUnixTimeStamp(ticket.CreatedDate)
            }));
            
            return result;
        }

        public override async Task<TicketModel> GetTicket(GetTicketInput request, ServerCallContext context)
        {
            var ticket = await _ticketRepository.GetAsync(request.TicketId);
            
            if (ticket is null) return null!;

            return new TicketModel
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                Description = ticket.Description,
                Topic = ticket.Topic,
                IsClosed = ticket.IsClosed,
                TicketAnswer = ticket.TicketAnswer,
                ClosedDate = ticket.ClosedDate is null ? 0 : Miscellaneous.DateTimeToUnixTimeStamp((DateTime)ticket.ClosedDate),
                CreatedDate = Miscellaneous.DateTimeToUnixTimeStamp(ticket.CreatedDate)
            };
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
