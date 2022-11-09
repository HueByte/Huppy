using AutoMapper;
using HuppyService.Core.Models;
using HuppyService.Core.Utilities;
using HuppyService.Service.Protos.Models;

namespace HuppyService.Service.Configuration
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CommandLogModel, CommandLog>()
                .ForMember(dest => dest.Date,
                    opt => opt.MapFrom(src => Miscellaneous.UnixTimeStampToUtcDateTime(src.Date)));

            CreateMap<CommandLog, CommandLogModel>()
                .ForMember(dest => dest.Date,
                    opt => opt.MapFrom(src => Miscellaneous.DateTimeToUnixTimeStamp(src.Date)));

            CreateMap<ServerModel, Server>()
                .ReverseMap();

            CreateMap<ServerRoomsModel, ServerRooms>()
                .ReverseMap();

            CreateMap<ReminderModel, Reminder>()
                .ForMember(dest => dest.RemindDate,
                    opt => opt.MapFrom(src => Miscellaneous.UnixTimeStampToUtcDateTime(src.RemindDate)));

            CreateMap<Reminder, ReminderModel>()
                .ForMember(dest => dest.RemindDate,
                    opt => opt.MapFrom(src => Miscellaneous.DateTimeToUnixTimeStamp(src.RemindDate)));

            CreateMap<TicketModel, Ticket>()
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(src => Miscellaneous.UnixTimeStampToUtcDateTime(src.CreatedDate)))
                .ForMember(dest => dest.ClosedDate,
                    opt => opt.MapFrom(src => Miscellaneous.UnixTimeStampToUtcDateTime(src.ClosedDate)));

            CreateMap<Ticket, TicketModel>()
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(src => Miscellaneous.DateTimeToUnixTimeStamp(src.CreatedDate)))
                .ForMember(dest => dest.ClosedDate,
                    opt => opt.MapFrom(src => Miscellaneous.DateTimeToUnixTimeStamp(src.ClosedDate)));
        }
    }
}
