using AutoMapper;
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
using System.Reflection.Metadata.Ecma335;

namespace HuppyService.Service.Services
{
    public class TicketService : TicketProto.TicketProtoBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IMapper _mapper;
        public TicketService(ITicketRepository ticketRepository, IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;   
        }

        public override async Task<TicketModel> AddTicket(AddTicketInput request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Description)) throw new ArgumentException("Ticket description cannot be null");

            //var ticket = _mapper.Map<Ticket>(request);
            //var ticket = ReflectionMapper.Map<Ticket>(request);

            Ticket ticket = new()
            {
                UserId = request.UserId,
                Topic = request.Topic,
                Description = request.Description,
                Id = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.UtcNow,
                TicketAnswer = null,
                IsClosed = false,
                ClosedDate = default
            };



            //Ticket ticket = new()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Topic = request.Topic,
            //    Description = request.Description,
            //    CreatedDate = DateTime.UtcNow,
            //    TicketAnswer = null,
            //    ClosedDate = null,
            //    IsClosed = false,
            //    UserId = request.UserId,
            //};

            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return _mapper.Map<TicketModel>(ticket);
            //return ReflectionMapper.Map<TicketModel>(ticket);
            //return new TicketModel()
            //{
            //    Id = ticket.Id,
            //    UserId = ticket.UserId,
            //    Description = ticket.Description,
            //    Topic = ticket.Topic,
            //    IsClosed = ticket.IsClosed,
            //    TicketAnswer = ticket.TicketAnswer,
            //    ClosedDate = ticket.ClosedDate is null ? 0 : Miscellaneous.DateTimeToUnixTimeStamp((DateTime)ticket.ClosedDate),
            //    CreatedDate = Miscellaneous.DateTimeToUnixTimeStamp(ticket.CreatedDate)
            //};
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

        public override async Task<Protos.Int32> GetCount(UserId request, ServerCallContext context)
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

            // TODO: use mapper
            //var result = new TicketModelCollection();
            //var temp = ReflectionMapper.Map<TicketModel>(tickets.ToArray());
            //result.TicketsModels.AddRange(temp);


            var result = new TicketModelCollection();
            result.TicketsModels.AddRange(_mapper.Map<TicketModel[]>(tickets));

            //result.TicketsModels.AddRange(tickets.Select(ticket => new TicketModel
            //{
            //    Id = ticket.Id,
            //    UserId = ticket.UserId,
            //    Description = ticket.Description,
            //    Topic = ticket.Topic,
            //    IsClosed = ticket.IsClosed,
            //    TicketAnswer = ticket.TicketAnswer,
            //    ClosedDate = ticket.ClosedDate == default ? 0 : Miscellaneous.DateTimeToUnixTimeStamp((DateTime)ticket.ClosedDate),
            //    CreatedDate = Miscellaneous.DateTimeToUnixTimeStamp(ticket.CreatedDate)
            //}));

            return result;
        }

        public override async Task<TicketModel> GetTicket(GetTicketInput request, ServerCallContext context)
        {
            var ticket = await _ticketRepository.GetAsync(request.TicketId);

            if (ticket is null) return null!;

            return _mapper.Map<TicketModel>(ticket);
            //return ReflectionMapper.Map<TicketModel>(ticket);
        }

        public override async Task<TicketModelCollection> GetTickets(Protos.Void request, ServerCallContext context)
        {
            var ticketsQuery = await _ticketRepository.GetAllAsync();
            var tickets = await ticketsQuery.ToListAsync();

            var result = new TicketModelCollection();
            result.TicketsModels.AddRange(_mapper.Map<TicketModel[]>(tickets));

            //result.TicketsModels.AddRange(tickets.Select(ticket => new TicketModel
            //{
            //    Id = ticket.Id,
            //    UserId = ticket.UserId,
            //    Description = ticket.Description,
            //    Topic = ticket.Topic,
            //    IsClosed = ticket.IsClosed,
            //    TicketAnswer = ticket.TicketAnswer,
            //    ClosedDate = ticket.ClosedDate == default ? 0 : Miscellaneous.DateTimeToUnixTimeStamp((DateTime)ticket.ClosedDate),
            //    CreatedDate = Miscellaneous.DateTimeToUnixTimeStamp(ticket.CreatedDate)
            //}));

            return result;
        }

        public override async Task<TicketModelCollection> GetUserPaginatedTickets(GetUserPaginatedTicketsInput request, ServerCallContext context)
        {
            var tickets = await (await _ticketRepository.GetAllAsync())
                .Where(ticket => ticket.UserId == request.UserId)
                .OrderBy(ticket => ticket.IsClosed)
                .ThenByDescending(ticket => ticket.CreatedDate)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var result = new TicketModelCollection();
            result.TicketsModels.AddRange(_mapper.Map<TicketModel[]>(tickets));

            //result.TicketsModels.AddRange(tickets.Select(ticket => new TicketModel
            //{
            //    Id = ticket.Id,
            //    UserId = ticket.UserId,
            //    Description = ticket.Description,
            //    Topic = ticket.Topic,
            //    IsClosed = ticket.IsClosed,
            //    TicketAnswer = ticket.TicketAnswer,
            //    ClosedDate = ticket.ClosedDate == default ? 0 : Miscellaneous.DateTimeToUnixTimeStamp((DateTime)ticket.ClosedDate),
            //    CreatedDate = Miscellaneous.DateTimeToUnixTimeStamp(ticket.CreatedDate)
            //}));

            return result;
        }

        public override async Task<TicketModelCollection> GetUserTickets(UserId request, ServerCallContext context)
        {
            var tickets = await (await _ticketRepository.GetAllAsync())
                .Where(ticket => ticket.UserId == request.Id)
                .ToListAsync();

            var result = new TicketModelCollection();
            result.TicketsModels.AddRange(_mapper.Map<TicketModel[]>(tickets));

            //result.TicketsModels.AddRange(tickets.Select(ticket => new TicketModel
            //{
            //    Id = ticket.Id,
            //    UserId = ticket.UserId,
            //    Description = ticket.Description,
            //    Topic = ticket.Topic,
            //    IsClosed = ticket.IsClosed,
            //    TicketAnswer = ticket.TicketAnswer,
            //    ClosedDate = ticket.ClosedDate == default ? 0 : Miscellaneous.DateTimeToUnixTimeStamp((DateTime)ticket.ClosedDate),
            //    CreatedDate = Miscellaneous.DateTimeToUnixTimeStamp(ticket.CreatedDate)
            //}));

            return result;
        }

        public override async Task<CommonResponse> RemoveTicket(StringId request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Id)) throw new ArgumentException("Ticket cannot be null or empty");

            await _ticketRepository.RemoveAsync(request.Id);
            await _ticketRepository.SaveChangesAsync();

            return new() { IsSuccess = true };
        }

        public override async Task<CommonResponse> UpdateTicket(TicketUpdateInput request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.TicketId) || string.IsNullOrEmpty(request.Description))
                throw new ArgumentException("Both ticked ID and ticket description cannot be null or empty");

            var ticket = await _ticketRepository.GetAsync(request.TicketId);

            if (ticket is null) throw new Exception("Ticket doesn't exist");

            ticket.Description = request.Description;

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return new() { IsSuccess = true };
        }
    }
}
