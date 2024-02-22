using System;
using Serilog;
using AutoMapper;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Diagnostics;
using SugarTalk.Core.Data;
using System.Threading.Tasks;
using SugarTalk.Core.Extensions;
using System.Collections.Generic;
using Google.Cloud.Translation.V2;
using SugarTalk.Core.Services.Jobs;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.AntMediaServer;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Core.Settings.Aliyun;
using SugarTalk.Core.Settings.LiveKit;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.User;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Events.Meeting.User;
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

        Task<GetMeetingHistoriesByUserResponse> GetMeetingHistoriesByUserAsync(GetMeetingHistoriesByUserRequest request, CancellationToken cancellationToken);
        
        Task HandleToRepeatMeetingAsync(
            Guid meetingId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            DateTimeOffset? utilDate,
            MeetingRepeatType repeatType, CancellationToken cancellationToken);
        
        Task<GetAppointmentMeetingsResponse> GetAppointmentMeetingsAsync(GetAppointmentMeetingsRequest request, CancellationToken cancellationToken);
        
        Task DeleteMeetingHistoryAsync(DeleteMeetingHistoryCommand command, CancellationToken cancellationToken);
    }
    
    public partial class MeetingService : IMeetingService
    {
        private const string appName = "LiveApp";

        private readonly IClock _clock;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly ISpeechClient _speechClient;
        private readonly ILiveKitClient _liveKitClient;
        private readonly TranslationClient _translationClient;
        private readonly IMeetingUtilService _meetingUtilService;
        private readonly IAccountDataProvider _accountDataProvider;
        private readonly IMeetingDataProvider _meetingDataProvider;
        private readonly ISugarTalkBackgroundJobClient _backgroundJobClient;
        private readonly ILiveKitServerUtilService _liveKitServerUtilService;
        private readonly IAntMediaServerUtilService _antMediaServerUtilService;

        private readonly AliYunOssSettings _aliYunOssSetting;
        private readonly LiveKitServerSetting _liveKitServerSetting;
        
        public MeetingService(
            IClock clock,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            ISpeechClient speechClient,
            ILiveKitClient liveKitClient,
            TranslationClient translationClient,
            IMeetingUtilService meetingUtilService,
            IMeetingDataProvider meetingDataProvider,
            IAccountDataProvider accountDataProvider,
            AliYunOssSettings aliYunOssSetting,
            LiveKitServerSetting liveKitServerSetting,
            ISugarTalkBackgroundJobClient backgroundJobClient,
            ILiveKitServerUtilService liveKitServerUtilService,
            IAntMediaServerUtilService antMediaServerUtilService)
        {
            _clock = clock;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _speechClient = speechClient;
            _liveKitClient = liveKitClient;
            _translationClient = translationClient;
            _meetingUtilService = meetingUtilService;
            _accountDataProvider = accountDataProvider;
            _meetingDataProvider = meetingDataProvider;
            _backgroundJobClient = backgroundJobClient;
            _aliYunOssSetting = aliYunOssSetting;
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
            if (command.AppointmentType == MeetingAppointmentType.Appointment)
            {
                if (command.RepeatType != MeetingRepeatType.None)
                    await HandleToRepeatMeetingAsync(
                        meeting.Id,
                        command.StartDate,
                        command.EndDate,
                        command.UtilDate,
                        command.RepeatType, cancellationToken).ConfigureAwait(false);
                
                await _meetingDataProvider.PersistMeetingRepeatRuleAsync(new MeetingRepeatRule
                {
                    MeetingId = meeting.Id,
                    RepeatType = command.RepeatType,
                    RepeatUntilDate = command.UtilDate
                }, cancellationToken).ConfigureAwait(false);
            }

            await _meetingDataProvider.PersistMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);

            var meetingDto = _mapper.Map<MeetingDto>(meeting);
            meetingDto.RepeatType = command.RepeatType;
            
            return new MeetingScheduledEvent { Meeting = meetingDto };
        }
        
        public async Task<GetMeetingHistoriesByUserResponse> GetMeetingHistoriesByUserAsync(
            GetMeetingHistoriesByUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider
                .GetUserAccountAsync(_currentUser.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (user is null) throw new UnauthorizedAccessException();

            var (meetingHistoryList, totalCount) = await _meetingDataProvider
                .GetMeetingHistoriesByUserIdAsync(user.Id, request.PageSetting, cancellationToken).ConfigureAwait(false);

            return new GetMeetingHistoriesByUserResponse
            {
                MeetingHistoryList = meetingHistoryList,
                TotalCount = totalCount
            };
        }

        public async Task HandleToRepeatMeetingAsync(
            Guid meetingId, 
            DateTimeOffset startDate, 
            DateTimeOffset endDate, 
            DateTimeOffset? utilDate,
            MeetingRepeatType repeatType, CancellationToken cancellationToken)
        {
            if (utilDate.HasValue && utilDate.Value < _clock.Now)
                throw new CannotCreateRepeatMeetingWhenUtilDateIsBeforeNowException(); 
            
            var subMeetingList = GenerateSubMeetings(meetingId, startDate, endDate, utilDate, repeatType);
            
            await _meetingDataProvider.PersistMeetingSubMeetingsAsync(subMeetingList, cancellationToken).ConfigureAwait(false);
        }

        public async Task<GetMeetingByNumberResponse> GetMeetingByNumberAsync(GetMeetingByNumberRequest request,
            CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider
                .GetMeetingAsync(request.MeetingNumber, cancellationToken, request.IncludeUserSession).ConfigureAwait(false);

            meeting.AppName = appName;

            if (request.IncludeUserSession && meeting.UserSessions.Any())
            {
                if (meeting.UserSessions.All(x => x.UserId != _currentUser.Id))
                {
                    throw new UnauthorizedAccessException();
                }

                meeting.UserSessions = meeting.UserSessions
                    .Where(x => x.OnlineType == MeetingUserSessionOnlineType.Online).ToList();
            }
            
            return new GetMeetingByNumberResponse { Data = meeting };
        }

        public async Task<MeetingJoinedEvent> JoinMeetingAsync(JoinMeetingCommand command, CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider.GetMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting.IsPasswordEnabled)
            {
                await _meetingDataProvider
                    .CheckMeetingSecurityCodeAsync(meeting.Id, command.SecurityCode, cancellationToken).ConfigureAwait(false);
            }

            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser?.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (user is null) throw new UnauthorizedAccessException();

            meeting.MeetingTokenFromLiveKit = user.Issuer switch
            {
                UserAccountIssuer.Guest => _liveKitServerUtilService.GenerateTokenForGuest(user, meeting.MeetingNumber),
                UserAccountIssuer.Self or UserAccountIssuer.Wiltechs => _liveKitServerUtilService.GenerateTokenForJoinMeeting(user, meeting.MeetingNumber),
                _ => throw new ArgumentOutOfRangeException()
            };

            await _meetingDataProvider.UpdateMeetingIfRequiredAsync(meeting.Id, user.Id, cancellationToken).ConfigureAwait(false);
            
            await ConnectUserToMeetingAsync(user, meeting, command.IsMuted, cancellationToken).ConfigureAwait(false);
            
            //接入音色接口后 弃用
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
            var userSession = await _meetingDataProvider.GetMeetingUserSessionByMeetingIdAsync(
                command.MeetingId, command.MeetingSubId, _currentUser?.Id, cancellationToken).ConfigureAwait(false);

            if (userSession == null) return new MeetingOutedEvent();

            EnrichMeetingUserSessionForOutMeeting(userSession);
            
            await _meetingDataProvider.UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);

            await _meetingDataProvider.HandleMeetingStatusWhenOutMeetingAsync(
                userSession.UserId, command.MeetingId, command.MeetingSubId.Value, cancellationToken).ConfigureAwait(false);

            return new MeetingOutedEvent();
        }
        
        public async Task<MeetingEndedEvent> EndMeetingAsync(EndMeetingCommand command, CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider.GetMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting.MeetingMasterUserId != _currentUser.Id) throw new CannotEndMeetingWhenUnauthorizedException();

            await _meetingDataProvider.PersistMeetingHistoryAsync(meeting, cancellationToken).ConfigureAwait(false);
            
            // TODO: 更新会议结束时间, 会议时长，更新会议中的用户状态
            var updateMeeting = _mapper.Map<Meeting>(meeting);
            
            await _meetingDataProvider.MarkMeetingAsCompletedAsync(updateMeeting, cancellationToken).ConfigureAwait(false);
            
            var attendingUserSessionIds = meeting.UserSessions.Select(x => x.Id).ToList();
            
            var updateMeetingUserSessions =
                await _meetingDataProvider.GetMeetingUserSessionsByIdsAndMeetingIdAsync(attendingUserSessionIds, meeting.Id, cancellationToken).ConfigureAwait(false);

            await _meetingDataProvider.UpdateUserSessionsAtMeetingEndAsync(updateMeeting, updateMeetingUserSessions, cancellationToken).ConfigureAwait(false);
            
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
                userSession = GenerateNewUserSessionFromUser(user, meeting.Id, meeting.MeetingSubId, isMuted ?? false);
                
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
                userSession.MeetingSubId = meeting.MeetingSubId;
                userSession.OnlineType = MeetingUserSessionOnlineType.Online;

                await _meetingDataProvider.UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);

                var updateUserSession = _mapper.Map<MeetingUserSessionDto>(userSession);

                updateUserSession.UserName = user.UserName;

                if (userSession.UserId == meeting.MeetingMasterUserId)
                {
                    updateUserSession.IsMeetingMaster = true;
                }
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

            await _meetingDataProvider.DeleteMeetingSubMeetingsAsync(updateMeeting.Id, cancellationToken).ConfigureAwait(false);
            
            if (command.AppointmentType == MeetingAppointmentType.Appointment && command.RepeatType != MeetingRepeatType.None)
            {
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

        private MeetingUserSession GenerateNewUserSessionFromUser(UserAccountDto user, Guid meetingId,
            Guid? meetingSubId, bool isMuted)
        {
            return new MeetingUserSession
            {
                UserId = user.Id,
                IsMuted = isMuted,
                MeetingId = meetingId,
                Status = MeetingAttendeeStatus.Present,
                FirstJoinTime = _clock.Now.ToUnixTimeSeconds(),
                TotalJoinCount = 1,
                MeetingSubId = meetingSubId
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

        public async Task DeleteMeetingHistoryAsync(DeleteMeetingHistoryCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (user is null) throw new UnauthorizedAccessException();
            
            await _meetingDataProvider.DeleteMeetingHistoryAsync(command.MeetingHistoryIds, user.Id, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteMeetingRecordAsync(DeleteMeetingRecordCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (user is null) throw new UnauthorizedAccessException();
            
            await _meetingDataProvider.DeleteMeetingRecordAsync(command.MeetingRecordIds, cancellationToken).ConfigureAwait(false);
        }
        
        private void EnrichMeetingUserSessionForOutMeeting(MeetingUserSession userSession)
        {
            var lastQuitTimeBeforeChange =
                userSession.LastQuitTime ??
                userSession.FirstJoinTime ??
                userSession.CreatedDate.ToUnixTimeSeconds();
            
            userSession.OnlineType = MeetingUserSessionOnlineType.OutMeeting;
            userSession.LastQuitTime = _clock.Now.ToUnixTimeSeconds();
            userSession.CumulativeTime = (userSession.CumulativeTime ?? 0) + (userSession.LastQuitTime - lastQuitTimeBeforeChange);
        }
    }
}