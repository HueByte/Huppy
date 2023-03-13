using AutoMapper;
using Grpc.Core;
using HuppyService.Core.Interfaces.IRepositories;
using HuppyService.Core.Models;
using HuppyService.Core.Utilities;
using HuppyService.Infrastructure.Repositories;
using HuppyService.Service.Protos;
using HuppyService.Service.Protos.Models;
using Microsoft.EntityFrameworkCore;

namespace HuppyService.Service.Services
{
    public class ReminderService : ReminderProto.ReminderProtoBase
    {
        private readonly ILogger _logger;
        private readonly IReminderRepository _reminderRepository;
        private readonly IMapper _mapper;
        public ReminderService(IReminderRepository reminderRepository, ILogger<ReminderService> logger, IMapper mapper)
        {
            _reminderRepository = reminderRepository;
            _logger = logger;
            _mapper = mapper;   
        }

        public override async Task<ReminderModelCollection> GetSortedUserReminders(SortedUserRemindersInput request, ServerCallContext context)
        {
            var query = await _reminderRepository.GetAllAsync();
            var reminders = await query
                .Where(reminder => reminder.UserId == request.UserId)
                .OrderBy(e => e.RemindDate)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            ReminderModelCollection? result = new();
            result.ReminderModels.AddRange(_mapper.Map<ReminderModel[]>(reminders));

            //result.ReminderModels.AddRange(reminders.Select(reminder => new ReminderModel
            //{
            //    Id = reminder.Id,
            //    Message = reminder.Message,
            //    UserId = reminder.UserId,
            //    RemindDate = Miscellaneous.DateTimeToUnixTimeStamp(reminder.RemindDate)
            //}).ToList());

            return result;
        }

        public override async Task<Protos.Int32> GetRemindersCount(Protos.UserId request, ServerCallContext context)
        {
            var query = await _reminderRepository.GetAllAsync();
            return new Protos.Int32 () { Number = await query.Where(e => e.UserId == request.Id).CountAsync() };
        }

        public override async Task<ReminderModel?> GetReminder(GetReminderInput request, ServerCallContext context)
        {
            var remindersQuery = await _reminderRepository.GetAllAsync();
            var reminder = await remindersQuery.FirstOrDefaultAsync(r => r.Id == request.ReminderId && r.UserId == request.UserId);

            if (reminder is null) return null;

            return _mapper.Map<ReminderModel?>(reminder); 

            //return new ReminderModel()
            //{
            //    Id = reminder.Id,
            //    Message = reminder.Message,
            //    RemindDate = Miscellaneous.DateTimeToUnixTimeStamp(reminder.RemindDate),
            //    UserId = reminder.UserId
            //};
        }

        public override async Task<ReminderModel> AddReminder(ReminderModel request, ServerCallContext context)
        {
            //var reminderDate = Miscellaneous.UnixTimeStampToUtcDateTime(request.RemindDate).ToUniversalTime();
            //Reminder reminder = new()
            //{
            //    Message = request.Message,
            //    RemindDate = reminderDate,
            //    UserId = request.UserId
            //};

            var reminder = _mapper.Map<Reminder>(request);

            var result = await _reminderRepository.AddAsync(reminder);
            await _reminderRepository.SaveChangesAsync();

            if (!result) throw new Exception("Failed to create reminder");

            _logger.LogInformation("Added reminder for [{user}] at [{date}] UTC", request.UserId, reminder.RemindDate);

            //return new ReminderModel() { Id = reminder.Id, Message = reminder.Message, RemindDate = request.RemindDate, UserId = reminder.UserId };
            return _mapper.Map<ReminderModel>(reminder);
        }

        public override async Task<ReminderModelCollection?> GetReminderBatch(ReminderBatchInput request, ServerCallContext context)
        {
            _logger.LogInformation("Registering fresh bulk of reminders");

            var remindDateEnd = Miscellaneous.UnixTimeStampToUtcDateTime(request.EndDate);

            var remindersQueryable = await _reminderRepository.GetAllAsync();

            var reminders = await remindersQueryable
                .Where(reminder => reminder.RemindDate < remindDateEnd)
                .ToListAsync();

            if (!reminders.Any()) return new();

            ReminderModelCollection? result = new();
            result.ReminderModels.AddRange(_mapper.Map<ReminderModel[]>(reminders));

            //result.ReminderModels.AddRange(reminders.Select(reminder => new ReminderModel
            //{
            //    Id = reminder.Id,
            //    Message = reminder.Message,
            //    UserId = reminder.UserId,
            //    RemindDate = Miscellaneous.DateTimeToUnixTimeStamp(reminder.RemindDate)
            //}).ToList());

            return result;
        }

        public override async Task<ReminderModelCollection> GetUserReminders(Protos.UserId request, ServerCallContext context)
        {
            var remindersQuery = await _reminderRepository.GetAllAsync();

            var reminders = await remindersQuery.Where(reminder => reminder.UserId == request.Id)
                                  .ToListAsync();

            ReminderModelCollection result = new();
            result.ReminderModels.AddRange(_mapper.Map<ReminderModel[]>(reminders));

            //result.ReminderModels.AddRange(reminders.Select(reminder => new ReminderModel()
            //{
            //    Id = reminder.Id,
            //    Message = reminder.Message,
            //    UserId = reminder.UserId,
            //    RemindDate = Miscellaneous.DateTimeToUnixTimeStamp(reminder.RemindDate)
            //}));

            return result;
        }

        public override async Task<CommonResponse> RemoveReminder(ReminderModel request, ServerCallContext context)
        {
            var reminder = _mapper.Map<Reminder>(request);
            //Reminder reminder = new()
            //{
            //    Message = request.Message,
            //    RemindDate = Miscellaneous.UnixTimeStampToUtcDateTime(request.RemindDate),
            //    UserId = request.UserId
            //};

            await _reminderRepository.RemoveAsync(reminder);
            await _reminderRepository.SaveChangesAsync();

            return new CommonResponse() { IsSuccess = true };
        }

        public override async Task<CommonResponse> RemoveReminderRange(RemoveReminderRangeInput request, ServerCallContext context)
        {
            if (request.Ids is null || !(request.Ids.Count > 0)) return new CommonResponse() { IsSuccess = true };

            await _reminderRepository.RemoveRangeAsync(request.Ids);
            await _reminderRepository.SaveChangesAsync();

            return new CommonResponse() { IsSuccess = true };
        }
    }
}
