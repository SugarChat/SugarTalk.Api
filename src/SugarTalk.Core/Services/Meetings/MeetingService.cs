using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Serilog;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Extensions;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.AntMediaServer;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Core.Settings.LiveKit;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.User;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Messages.Events.Meeting.User;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Requests.Meetings.User;

namespace SugarTalk.Core.Services.Meetings
{
    public partial interface IMeetingService : IScopedDependency
    {
        Task<MeetingScheduledEvent> ScheduleMeetingAsync(
            ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken);

        Task<GetMeetingByNumberResponse> GetMeetingByNumberAsync(
            GetMeetingByNumberRequest request, CancellationToken cancellationToken = default);

        Task<MeetingJoinedEvent> JoinMeetingAsync(
            JoinMeetingCommand command, CancellationToken cancellationToken);
        
        Task<MeetingOutedEvent> OutMeetingAsync(
            OutMeetingCommand command, CancellationToken cancellationToken);

        Task<MeetingEndedEvent> EndMeetingAsync(
            EndMeetingCommand command, CancellationToken cancellationToken = default);

        Task ConnectUserToMeetingAsync(
            UserAccountDto user, MeetingDto meeting, bool? isMuted = null, CancellationToken cancellationToken = default);

        Task<UpdateMeetingResponse> UpdateMeetingAsync(UpdateMeetingCommand command, CancellationToken cancellationToken);
        
        Task<MeetingUserSettingAddOrUpdatedEvent> AddOrUpdateMeetingUserSettingAsync(
            AddOrUpdateMeetingUserSettingCommand command, CancellationToken cancellationToken);

        Task<GetMeetingUserSettingResponse> GetMeetingUserSettingAsync(GetMeetingUserSettingRequest request, CancellationToken cancellationToken);

        Task HandleToRepeatMeetingAsync(
            Guid meetingId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            DateTimeOffset? utilDate,
            MeetingRepeatType repeatType, CancellationToken cancellationToken);
        
        Task<GetAppointmentMeetingsResponse> GetAppointmentMeetingsAsync(GetAppointmentMeetingsRequest request, CancellationToken cancellationToken);
    }
    
    public partial class MeetingService : IMeetingService
    {
        private const string appName = "LiveApp";

        private readonly IClock _clock;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly ISpeechClient _speechClient;
        private readonly IAccountDataProvider _accountDataProvider;
        private readonly IMeetingDataProvider _meetingDataProvider;
        private readonly ILiveKitServerUtilService _liveKitServerUtilService;
        private readonly IAntMediaServerUtilService _antMediaServerUtilService;

        private readonly LiveKitServerSetting _liveKitServerSetting;
        
        public MeetingService(
            IClock clock,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            ISpeechClient speechClient,
            IMeetingDataProvider meetingDataProvider,
            IAccountDataProvider accountDataProvider,
            LiveKitServerSetting liveKitServerSetting,
            ILiveKitServerUtilService liveKitServerUtilService,
            IAntMediaServerUtilService antMediaServerUtilService)
        {
            _clock = clock;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _speechClient = speechClient;
            _accountDataProvider = accountDataProvider;
            _meetingDataProvider = meetingDataProvider;
            _liveKitServerSetting = liveKitServerSetting;
            _liveKitServerUtilService = liveKitServerUtilService;
            _antMediaServerUtilService = antMediaServerUtilService;
        }
        
        public async Task<MeetingScheduledEvent> ScheduleMeetingAsync(ScheduleMeetingCommand command, CancellationToken cancellationToken)
        {
            var meetingNumber = GenerateMeetingNumber();
            
            var meeting = await GenerateMeetingInfoFromThirdPartyServicesAsync(meetingNumber, cancellationToken).ConfigureAwait(false);
            meeting = _mapper.Map(command, meeting);
            meeting.Id = Guid.NewGuid();
            meeting.MeetingMasterUserId = _currentUser.Id.Value;
            meeting.MeetingStreamMode = MeetingStreamMode.LEGACY;
            meeting.SecurityCode = !string.IsNullOrEmpty(command.SecurityCode) ? command.SecurityCode.ToSha256() : null;

            // 处理周期性预定会议生成的子会议
            if (command.AppointmentType == MeetingAppointmentType.Appointment && command.RepeatType != MeetingRepeatType.None)
            {
                await HandleToRepeatMeetingAsync(
                    meeting.Id,
                    command.StartDate,
                    command.EndDate,
                    command.UtilDate,
                    command.RepeatType, cancellationToken).ConfigureAwait(false);
            }

            await _meetingDataProvider.PersistMeetingRepeatRuleAsync(new MeetingRepeatRule
            {
                MeetingId = meeting.Id,
                RepeatType = command.RepeatType,
                RepeatUntilDate = command.UtilDate
            }, cancellationToken).ConfigureAwait(false);

            await _meetingDataProvider.PersistMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);

            var meetingDto = _mapper.Map<MeetingDto>(meeting);
            meetingDto.RepeatType = command.RepeatType;
            
            return new MeetingScheduledEvent { Meeting = meetingDto };
        }
        
        public async Task HandleToRepeatMeetingAsync(
            Guid meetingId, 
            DateTimeOffset startDate, 
            DateTimeOffset endDate, 
            DateTimeOffset? utilDate,
            MeetingRepeatType repeatType, CancellationToken cancellationToken)
        {
            var subMeetingList = GenerateSubMeetings(meetingId, startDate, endDate, utilDate, repeatType);
            
            await _meetingDataProvider.PersistMeetingSubMeetingsAsync(subMeetingList, cancellationToken).ConfigureAwait(false);
        }

        public async Task<GetMeetingByNumberResponse> GetMeetingByNumberAsync(GetMeetingByNumberRequest request,
            CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider
                .GetMeetingAsync(request.MeetingNumber, cancellationToken, request.IncludeUserSession).ConfigureAwait(false);

            meeting.AppName = appName;
            
            if (request.IncludeUserSession &&
                meeting != null && meeting.UserSessions.Any() &&
                meeting.UserSessions.All(x => x.UserId != _currentUser.Id.Value))
                throw new UnauthorizedAccessException();

            return new GetMeetingByNumberResponse { Data = meeting };
        }

        public async Task<MeetingJoinedEvent> JoinMeetingAsync(JoinMeetingCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            var meeting = await _meetingDataProvider.GetMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting.IsPasswordEnabled)
            {
                await _meetingDataProvider
                    .CheckMeetingSecurityCodeAsync(meeting.Id, command.SecurityCode, cancellationToken).ConfigureAwait(false);
            }

            meeting.MeetingTokenFromLiveKit = _liveKitServerUtilService.GenerateTokenForJoinMeeting(user, meeting.MeetingNumber);

            await _meetingDataProvider.UpdateMeetingStatusAsync(meeting.Id, cancellationToken).ConfigureAwait(false);
            
            await ConnectUserToMeetingAsync(user, meeting, command.IsMuted, cancellationToken).ConfigureAwait(false);
            
            //TODO：创建人加入会议时开启录制
            
            var userSetting = await _meetingDataProvider.DistributeLanguageForMeetingUserAsync(meeting.Id, cancellationToken).ConfigureAwait(false);
            
            Log.Information("SugarTalk get userSetting from JoinMeetingAsync :{userSetting}", JsonConvert.SerializeObject(userSetting));

            return new MeetingJoinedEvent
            {
                Meeting = meeting,
                MeetingUserSetting = _mapper.Map<MeetingUserSettingDto>(userSetting)
            };
        }
        
        public async Task<MeetingOutedEvent> OutMeetingAsync(OutMeetingCommand command, CancellationToken cancellationToken)
        {
            var userSession = await _meetingDataProvider
                .GetMeetingUserSessionByMeetingIdAsync(command.MeetingId, _currentUser.Id.Value, cancellationToken).ConfigureAwait(false);

            if (userSession == null) return new MeetingOutedEvent();

            // TODO: 更新用户退出会议时间, 会议时长

            return new MeetingOutedEvent();
        }
        
        public async Task<MeetingEndedEvent> EndMeetingAsync(EndMeetingCommand command, CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider.GetMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting.MeetingMasterUserId != _currentUser.Id) throw new CannotEndMeetingWhenUnauthorizedException();

            // TODO: 更新会议结束时间, 会议时长，更新会议中的用户状态
            var updateMeeting = _mapper.Map<Meeting>(meeting);
            
            await _meetingDataProvider.UpdateMeetingEndStatusAsync(updateMeeting, cancellationToken);
            
            var userSessionIds = meeting.UserSessions.Select(x => x.UserId).ToList();
            
            var updateMeetingUserSessions =
                await _meetingDataProvider.GetMeetingUserSessionsByIdsAndMeetingIdAsync(userSessionIds, meeting.Id, cancellationToken).ConfigureAwait(false);

            await _meetingDataProvider.UpdateMeetingEndUserSessionAsync(updateMeeting, updateMeetingUserSessions, cancellationToken).ConfigureAwait(false);
            
            return new MeetingEndedEvent
            {
                MeetingNumber = meeting.MeetingNumber,
                MeetingUserSessionIds = meeting.UserSessions.Select(x => x.Id).ToList()
            };
        }

        public async Task ConnectUserToMeetingAsync(
            UserAccountDto user, MeetingDto meeting, bool? isMuted = null, CancellationToken cancellationToken = default)
        {
            var userSession = meeting.UserSessions
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => _mapper.Map<MeetingUserSession>(x))
                .FirstOrDefault();

            if (userSession == null)
            {
                userSession = GenerateNewUserSessionFromUser(user, meeting.Id, isMuted ?? false);

                await _meetingDataProvider.AddMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                var updateUserSession = _mapper.Map<MeetingUserSessionDto>(userSession);

                updateUserSession.UserName = user.UserName;
                
                meeting.AddUserSession(updateUserSession);
            }
            else
            {
                if (isMuted.HasValue)
                    userSession.IsMuted = isMuted.Value;

                userSession.Status = MeetingAttendeeStatus.Present;
                userSession.FirstJoinTime = _clock.Now.ToUnixTimeSeconds();
                userSession.TotalJoinCount += 1;

                await _meetingDataProvider.UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                var updateUserSession = _mapper.Map<MeetingUserSessionDto>(userSession);

                updateUserSession.UserName = user.UserName;

                meeting.UpdateUserSession(updateUserSession);
            }
        }

        public async Task<UpdateMeetingResponse> UpdateMeetingAsync(UpdateMeetingCommand command, CancellationToken cancellationToken)
        {
            var currentUser = await _accountDataProvider
                .GetUserAccountAsync(_currentUser.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (currentUser is null) throw new UnauthorizedAccessException();
            
            var meeting = await _meetingDataProvider
                .GetMeetingByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);

            if (meeting is null) throw new MeetingNotFoundException();
            
            Log.Information("Meeting master userId:{masterId}, current userId{currentUserId}",
                meeting.MeetingMasterUserId, _currentUser.Id.Value);
            
            if (meeting.MeetingMasterUserId != _currentUser.Id.Value)
                throw new CannotUpdateMeetingWhenMasterUserIdMismatchException();

            var updateMeeting = _mapper.Map(command, meeting);

            if (!string.IsNullOrEmpty(command.SecurityCode))
                updateMeeting.SecurityCode = command.SecurityCode.ToSha256();

            if (command.AppointmentType == MeetingAppointmentType.Appointment && command.RepeatType != MeetingRepeatType.None)
            {
                await _meetingDataProvider.DeleteMeetingSubMeetingsAsync(updateMeeting.Id, cancellationToken).ConfigureAwait(false);
                
                var subMeetingList = GenerateSubMeetings(updateMeeting.Id, command.StartDate, command.EndDate, command.UtilDate, command.RepeatType);
                
                await _meetingDataProvider.UpdateMeetingRepeatRuleAsync(updateMeeting.Id, command.RepeatType, cancellationToken).ConfigureAwait(false);

                await _meetingDataProvider.PersistMeetingSubMeetingsAsync(subMeetingList, cancellationToken).ConfigureAwait(false);
            }
            
            await _meetingDataProvider.UpdateMeetingAsync(updateMeeting, cancellationToken).ConfigureAwait(false);

            return new UpdateMeetingResponse();
        }

        public async Task<MeetingUserSettingAddOrUpdatedEvent> AddOrUpdateMeetingUserSettingAsync(
            AddOrUpdateMeetingUserSettingCommand command, CancellationToken cancellationToken)
        {
            var response = new MeetingUserSettingAddOrUpdatedEvent();

            if (!_currentUser.Id.HasValue) return response;

            var userSetting = await _meetingDataProvider
                .GetMeetingUserSettingByUserIdAsync(_currentUser.Id.Value, command.MeetingId, cancellationToken).ConfigureAwait(false);
            
            if (userSetting is null)
            {
                await _meetingDataProvider.AddMeetingUserSettingAsync(new MeetingUserSetting
                {
                    UserId = _currentUser.Id.Value,
                    MeetingId = command.MeetingId,
                    TargetLanguageType = command.TargetLanguageType,
                }, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                userSetting.LastModifiedDate = DateTimeOffset.Now;
                userSetting.TargetLanguageType = command.TargetLanguageType;
                
                await _meetingDataProvider.UpdateMeetingUserSettingAsync(userSetting, cancellationToken).ConfigureAwait(false);
            }
            
            return response;
        }

        public async Task<GetMeetingUserSettingResponse> GetMeetingUserSettingAsync(GetMeetingUserSettingRequest request, CancellationToken cancellationToken)
        {
            if (!_currentUser.Id.HasValue) throw new UnauthorizedAccessException();

            var userAccounts = await _accountDataProvider
                .GetUserAccountsAsync(_currentUser.Id.Value, cancellationToken).ConfigureAwait(false);

            var userIds = userAccounts.Select(x => x.Id).ToList();
            
            var userSettings = await _meetingDataProvider.GetMeetingUserSettingsAsync(userIds, cancellationToken).ConfigureAwait(false);

            userSettings = userSettings.Where(x => x.MeetingId == request.MeetingId).ToList();
            
            if (userSettings is not { Count: > 0 }) return new GetMeetingUserSettingResponse();

            return new GetMeetingUserSettingResponse
            {
                Data = _mapper.Map<MeetingUserSettingDto>(userSettings.FirstOrDefault())
            };
        }

        private async Task<Meeting> GenerateMeetingInfoFromThirdPartyServicesAsync(string meetingNumber, CancellationToken cancellationToken)
        {
            var meeting = new Meeting();
            
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            var token = _liveKitServerUtilService.GenerateTokenForCreateMeeting(user, meetingNumber);

            Log.Information("Generate liveKit token:{token}", token);

            var liveKitResponse = await _liveKitServerUtilService
                .CreateMeetingAsync(meetingNumber, token, cancellationToken: cancellationToken).ConfigureAwait(false);

            Log.Information("Create to meeting from liveKit response:{response}", liveKitResponse);
            
            if (liveKitResponse is null && string.IsNullOrEmpty(token)) throw new CannotCreateMeetingException();
            
            meeting.MeetingNumber = liveKitResponse?.RoomInfo?.MeetingNumber ?? meetingNumber;

            return meeting;
        }
        
        private List<MeetingSubMeeting> GenerateSubMeetings(
            Guid meetingId, DateTimeOffset startDate, DateTimeOffset endDate, DateTimeOffset? utilDate, MeetingRepeatType repeatType)
        {
            var subMeetingList = new List<MeetingSubMeeting>();
            
            var loopCount = utilDate.HasValue ? CalculateLoopCount(startDate, utilDate.Value, repeatType) : 7;

            for (var i = 0; i < loopCount; i++)
            {
                // 跳过非工作日，不计入循环次数
                if (repeatType == MeetingRepeatType.EveryWeekday && !IsWorkday(startDate))
                {
                    --i;
                }
                else
                {
                    subMeetingList.Add(new MeetingSubMeeting
                    {
                        Id = Guid.NewGuid(),
                        MeetingId = meetingId,
                        StartTime = startDate.ToUnixTimeSeconds(),
                        EndTime = endDate.ToUnixTimeSeconds()
                    });
                }

                IncrementDates(ref startDate, ref endDate, repeatType);
            }

            return subMeetingList;
        }
        
        private int CalculateLoopCount(DateTimeOffset startDate, DateTimeOffset utilDate, MeetingRepeatType repeatType)
        {
            var count = 0;
            
            while (startDate <= utilDate)
            {
                if (repeatType != MeetingRepeatType.EveryWeekday || IsWorkday(startDate))
                {
                    ++count;
                }

                startDate = GetNextMeetingDate(startDate, repeatType);
            }
            
            return count;
        }

        private void IncrementDates(ref DateTimeOffset startDate, ref DateTimeOffset endDate, MeetingRepeatType repeatType)
        {
            startDate = GetNextMeetingDate(startDate, repeatType);
            endDate = GetNextMeetingDate(endDate, repeatType);
        }
        
        private DateTimeOffset GetNextMeetingDate(DateTimeOffset currentDate, MeetingRepeatType repeatType)
        {
            var nextDate = currentDate;

            switch (repeatType)
            {
                case MeetingRepeatType.Daily:
                case MeetingRepeatType.EveryWeekday:
                    nextDate = currentDate.AddDays(1);
                    break;
                case MeetingRepeatType.Weekly:
                    nextDate = currentDate.AddDays(7);
                    break;
                case MeetingRepeatType.BiWeekly:
                    nextDate = currentDate.AddDays(14);
                    break;
                case MeetingRepeatType.Monthly:
                    nextDate = currentDate.AddMonths(1);
                    break;
            }

            // Adjust for weekdays required
            if (repeatType == MeetingRepeatType.EveryWeekday && nextDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                var daysToAdd = nextDate.DayOfWeek is DayOfWeek.Saturday ? 2 : 1;
                nextDate = nextDate.AddDays(daysToAdd);
            }

            return nextDate;
        }
        
        private bool IsWorkday(DateTimeOffset date)
        {
            var workday = date.DayOfWeek;
            return workday != DayOfWeek.Saturday && workday != DayOfWeek.Sunday;
        }

        private string GenerateMeetingNumber()
        {
            var result = new StringBuilder();
            for (var i = 0; i < 5; i++)
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                result.Append(r.Next(0, 10));
            }
            return result.ToString();
        }
        
        private MeetingUserSession GenerateNewUserSessionFromUser(UserAccountDto user, Guid meetingId, bool isMuted)
        {
            return new MeetingUserSession
            {
                UserId = user.Id,
                IsMuted = isMuted,
                MeetingId = meetingId,
                Status = MeetingAttendeeStatus.Present,
                FirstJoinTime = _clock.Now.ToUnixTimeSeconds(),
                TotalJoinCount = 1
            };
        }
        
        public async Task<GetAppointmentMeetingsResponse> GetAppointmentMeetingsAsync(GetAppointmentMeetingsRequest request, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (user is null) throw new UnreachableException();

            var (count, records) = await _meetingDataProvider.GetAppointmentMeetingsByUserIdAsync(request, cancellationToken).ConfigureAwait(false);
        
            return new GetAppointmentMeetingsResponse
            {
                Data = new GetAppointmentMeetingsResponseDto()
                {
                    Count = count,
                    Records = records
                }
            };
        }
    }
}