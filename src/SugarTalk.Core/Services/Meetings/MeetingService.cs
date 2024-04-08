using System;
using Serilog;
using AutoMapper;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using System.Diagnostics;
using SugarTalk.Core.Data;
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
using SugarTalk.Core.Services.Http;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Core.Settings.Meeting;
using SugarTalk.Messages.Commands.Meetings.Speak;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Enums.Speech;
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
        
        Task<ChatRoomSettingAddOrUpdateEvent> AddOrUpdateChatRoomSettingAsync(
            AddOrUpdateChatRoomSettingCommand command, CancellationToken cancellationToken);

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
        
        Task<AppointmentMeetingCanceledEvent> CancelAppointmentMeetingAsync(CancelAppointmentMeetingCommand command, CancellationToken cancellationToken);
        
        Task<GetMeetingInviteInfoResponse> GetMeetingInviteInfoAsync(GetMeetingInviteInfoRequest request, CancellationToken cancellationToken);
        
        Task HandleMeetingStatusWhenOutMeetingAsync(int userId, Guid meetingId, Guid? meetingSubId = null, CancellationToken cancellationToken = default);
        
        Task<MeetingSwitchEaResponse> UpdateMeetingChatResponseAsync(MeetingSwitchEaCommand switchEaCommand, CancellationToken cancellationToken);
    }
    
    public partial class MeetingService : IMeetingService
    {
        private const string appName = "LiveApp";

        private readonly IClock _clock;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly IOpenAiService _openAiService;
        private readonly ISpeechClient _speechClient;
        private readonly ILiveKitClient _liveKitClient;
        private readonly TranslationClient _translationClient;
        private readonly IMeetingUtilService _meetingUtilService;
        private readonly IAccountDataProvider _accountDataProvider;
        private readonly IMeetingDataProvider _meetingDataProvider;
        private readonly ISugarTalkHttpClientFactory _httpClientFactory;
        private readonly ISugarTalkBackgroundJobClient _backgroundJobClient;
        private readonly ILiveKitServerUtilService _liveKitServerUtilService;
        private readonly IAntMediaServerUtilService _antMediaServerUtilService;
        private readonly ISugarTalkBackgroundJobClient _sugarTalkBackgroundJobClient;

        private readonly AliYunOssSettings _aliYunOssSetting;
        private readonly LiveKitServerSetting _liveKitServerSetting;
        private readonly MeetingInfoSettings _meetingInfoSettings;
        
        public MeetingService(
            IClock clock,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            IOpenAiService openAiService,
            ISpeechClient speechClient,
            ILiveKitClient liveKitClient,
            AliYunOssSettings aliYunOssSetting,
            TranslationClient translationClient,
            IMeetingUtilService meetingUtilService,
            IMeetingDataProvider meetingDataProvider,
            IAccountDataProvider accountDataProvider,
            LiveKitServerSetting liveKitServerSetting,
            ISugarTalkHttpClientFactory httpClientFactory,
            ISugarTalkBackgroundJobClient backgroundJobClient,
            ILiveKitServerUtilService liveKitServerUtilService,
            IAntMediaServerUtilService antMediaServerUtilService,
            ISugarTalkBackgroundJobClient sugarTalkBackgroundJobClient, 
            MeetingInfoSettings meetingInfoSettings)
        {
            _clock = clock;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _openAiService = openAiService;
            _speechClient = speechClient;
            _liveKitClient = liveKitClient;
            _aliYunOssSetting = aliYunOssSetting;
            _httpClientFactory = httpClientFactory;
            _translationClient = translationClient;
            _meetingUtilService = meetingUtilService;
            _accountDataProvider = accountDataProvider;
            _meetingDataProvider = meetingDataProvider;
            _backgroundJobClient = backgroundJobClient;
            _liveKitServerSetting = liveKitServerSetting;
            _liveKitServerUtilService = liveKitServerUtilService;
            _antMediaServerUtilService = antMediaServerUtilService;
            _sugarTalkBackgroundJobClient = sugarTalkBackgroundJobClient;
            _meetingInfoSettings = meetingInfoSettings;
        }
        
        public async Task<MeetingScheduledEvent> ScheduleMeetingAsync(ScheduleMeetingCommand command, CancellationToken cancellationToken)
        {
            var meetingNumber = GenerateMeetingNumber();
            
            var meeting = await GenerateMeetingInfoFromThirdPartyServicesAsync(meetingNumber, cancellationToken).ConfigureAwait(false);
            meeting = _mapper.Map(command, meeting);
            meeting.Id = Guid.NewGuid();
            meeting.Status = MeetingStatus.Pending;
            meeting.MeetingMasterUserId = _currentUser.Id.Value;
            meeting.MeetingStreamMode = MeetingStreamMode.LEGACY;
            meeting.SecurityCode = !string.IsNullOrEmpty(command.SecurityCode) ? command.SecurityCode.ToSha256() : null;
            meeting.Password = command.SecurityCode;
            
            switch (command.AppointmentType)
            {
                case MeetingAppointmentType.Quick:
                    meeting.Title = _currentUser.Name + "的快速会议";
                    break;
                // 处理周期性预定会议生成的子会议
                case MeetingAppointmentType.Appointment:
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
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
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
                .GetMeetingHistoriesByUserIdAsync(user.Id, request.Keyword, request.PageSetting, cancellationToken).ConfigureAwait(false);

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
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser?.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (user is null) throw new UnauthorizedAccessException();
            
            await _meetingDataProvider.CheckUserKickedFromMeetingAsync(command.MeetingNumber, user.Id, cancellationToken).ConfigureAwait(false);
            
            var meeting = await _meetingDataProvider.GetMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting.MeetingMasterUserId != user.Id && meeting.IsPasswordEnabled)
            {
                await _meetingDataProvider
                    .CheckMeetingSecurityCodeAsync(meeting.Id, command.SecurityCode, cancellationToken).ConfigureAwait(false);
            }

            CheckJoinMeetingConditions(meeting, user);

            await OutLiveKitExistedUserAsync(meetingNumber: meeting.MeetingNumber, userName: user.UserName, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            await ConnectUserToMeetingAsync(user, meeting, command.IsMuted, cancellationToken).ConfigureAwait(false);

            await _meetingDataProvider.UpdateMeetingIfRequiredAsync(meeting.Id, user.Id, cancellationToken).ConfigureAwait(false);

            meeting.Status = MeetingStatus.InProgress;
            
            //接入音色接口后 弃用
            var userSetting = await _meetingDataProvider.DistributeLanguageForMeetingUserAsync(meeting.Id, cancellationToken).ConfigureAwait(false);

            Log.Information("SugarTalk get userSetting from JoinMeetingAsync :{userSetting}", JsonConvert.SerializeObject(userSetting));
            
            await _meetingDataProvider.AddMeetingChatRoomSettingAsync(new MeetingChatRoomSetting
            {
                IsSystem = true,
                UserId = userSetting.UserId,
                MeetingId = meeting.Id,
                VoiceId = userSetting.TargetLanguageType switch
                {
                    SpeechTargetLanguageType.Cantonese => ((int)userSetting.CantoneseToneType).ToString(),
                    SpeechTargetLanguageType.Mandarin => ((int)userSetting.MandarinToneType).ToString(),
                    SpeechTargetLanguageType.English => ((int)userSetting.EnglishToneType).ToString(),
                    SpeechTargetLanguageType.Spanish => ((int)userSetting.SpanishToneType).ToString(),
                    _ => string.Empty
                }
            }, true, cancellationToken).ConfigureAwait(false);
            
            return new MeetingJoinedEvent
            {
                UserId = user.Id,
                Meeting = meeting,
                MeetingUserSetting = _mapper.Map<MeetingUserSettingDto>(userSetting)
            };
        }

        private void CheckJoinMeetingConditions(MeetingDto meeting, UserAccountDto user)
        {
            //加入未在进行中的会议时判断当前用户是否是主持人，如果不是则判断会议开始时间、会议中是否有主持人。都不符合则抛出异常
            if (meeting.MeetingMasterUserId == user.Id) return;
            
            var now = _clock.Now.ToUnixTimeSeconds();

            var hasUserSessions = meeting.UserSessions.Count > 0;
            var hasMeetingMaster = meeting.UserSessions.Any(x => x.IsMeetingMaster);
            
            if (now >= meeting.StartDate && now <= meeting.EndDate && (hasMeetingMaster || hasUserSessions))
            {
                return;
            }

            if (meeting.AppointmentType == MeetingAppointmentType.Quick && (meeting.Status == MeetingStatus.Pending || meeting.Status == MeetingStatus.InProgress))
            {
                return;
            }
    
            throw new CannotJoinMeetingWhenMeetingClosedException();
        }

        private async Task OutLiveKitExistedUserAsync(string meetingNumber = null, string userName = null, Guid? meetingId = null, CancellationToken cancellationToken = default)
        {
            if (meetingId.HasValue)
                meetingNumber = _mapper.Map<MeetingDto>(await _meetingDataProvider.GetMeetingByIdAsync(meetingId.Value, cancellationToken).ConfigureAwait(false)).MeetingNumber;
            
            var token = _liveKitServerUtilService.GetMeetingInfoPermission(meetingNumber);
            
            var meetingExistUser = await _liveKitClient.ListParticipantsAsync(
                new ListParticipantsRequestDto{Room = meetingNumber, Token = token}, cancellationToken).ConfigureAwait(false);
            
            if (meetingExistUser != null)
            {
                var existUser = meetingExistUser.Participants.FirstOrDefault(x => x.Name == userName);

                if (existUser != null)
                {
                    await _liveKitClient.RemoveParticipantAsync(
                        new RemoverParticipantRequestDto{Room = meetingNumber, Identity = existUser.Identity, Token = token}, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<MeetingOutedEvent> OutMeetingAsync(OutMeetingCommand command, CancellationToken cancellationToken)
        {
            var userSession = await _meetingDataProvider.GetMeetingUserSessionByMeetingIdAsync(
                command.MeetingId, command.MeetingSubId, _currentUser?.Id, cancellationToken).ConfigureAwait(false);

            if (userSession == null) return new MeetingOutedEvent();

            EnrichMeetingUserSessionForOutMeeting(userSession);
            
            await _meetingDataProvider.UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
            
            await OutLiveKitExistedUserAsync(userName: _currentUser?.Name, meetingId: command.MeetingId, cancellationToken: cancellationToken).ConfigureAwait(false);

            await HandleMeetingStatusWhenOutMeetingAsync(
                userSession.UserId, command.MeetingId, command.MeetingSubId, cancellationToken).ConfigureAwait(false);

            return new MeetingOutedEvent();
        }

        public async Task HandleMeetingStatusWhenOutMeetingAsync(
            int userId, Guid meetingId, Guid? meetingSubId = null, CancellationToken cancellationToken = default)
        {
            var meeting = await _meetingDataProvider.GetMeetingByIdAsync(meetingId, cancellationToken).ConfigureAwait(false);
            
            if (meeting is null) throw new MeetingNotFoundException();
            
            var attendingUserSessionsExceptCurrentUser = 
                await _meetingDataProvider.GetMeetingUserSessionAsync(meetingId, meetingSubId, cancellationToken).ConfigureAwait(false);

            attendingUserSessionsExceptCurrentUser = attendingUserSessionsExceptCurrentUser
                .Where(x => x.UserId != userId)
                .Where(x => x.Status == MeetingAttendeeStatus.Present && x.OnlineType == MeetingUserSessionOnlineType.Online).ToList();
            
            if (attendingUserSessionsExceptCurrentUser is { Count: > 0 }) return;
            
            meeting.Status = MeetingStatus.InProgress;

            switch (meeting.AppointmentType)
            {
                case MeetingAppointmentType.Quick:
                    meeting.Status = MeetingStatus.Completed;
                    break;
                case MeetingAppointmentType.Appointment:
                {
                    if (_clock.Now < DateTimeOffset.FromUnixTimeSeconds(meeting.StartDate))
                    {
                        meeting.Status = MeetingStatus.Pending;
                    }

                    if (_clock.Now > DateTimeOffset.FromUnixTimeSeconds(meeting.EndDate))
                    {
                        meeting.Status = MeetingStatus.Completed;
                    }

                    break;
                }
               
                default: throw new ArgumentOutOfRangeException();
            }

            await _meetingDataProvider.UpdateMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MeetingEndedEvent> EndMeetingAsync(EndMeetingCommand command, CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider.GetMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting.MeetingMasterUserId != _currentUser.Id) throw new CannotEndMeetingWhenUnauthorizedException();

            await _meetingDataProvider.PersistMeetingHistoryAsync(meeting, cancellationToken).ConfigureAwait(false);
            
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
                .Select(x => _mapper.Map<MeetingUserSession>(x))
                .FirstOrDefault();

            if (userSession == null)
            {
                userSession = GenerateNewUserSessionFromUser(user, meeting.Id, meeting.MeetingSubId, isMuted ?? false);
                
                if (user.Issuer == UserAccountIssuer.Guest) HandleGuestNameForUserSession(meeting, userSession);
                
                await _meetingDataProvider.AddMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);

                var addUserSession = _mapper.Map<MeetingUserSessionDto>(userSession);
                addUserSession.UserName = user.UserName;

                Log.Information("ConnectUserToMeetingAsync: addUserSession:{addUserSession}", addUserSession);

                meeting.AddUserSession(addUserSession);
                
                meeting.MeetingTokenFromLiveKit = user.Issuer switch
                {
                    UserAccountIssuer.Guest => _liveKitServerUtilService.GenerateTokenForGuest(addUserSession.GuestName, addUserSession.GuestName, meeting.MeetingNumber),
                    UserAccountIssuer.Self or UserAccountIssuer.Wiltechs => _liveKitServerUtilService.GenerateTokenForJoinMeeting(user, meeting.MeetingNumber),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            else
            {
                if (isMuted.HasValue)
                    userSession.IsMuted = isMuted.Value;

                userSession.Status = MeetingAttendeeStatus.Present;
                userSession.LastJoinTime = _clock.Now.ToUnixTimeSeconds();
                userSession.TotalJoinCount += 1;
                userSession.MeetingSubId = meeting.MeetingSubId;
                userSession.OnlineType = MeetingUserSessionOnlineType.Online;

                if (user.Issuer == UserAccountIssuer.Guest) HandleGuestNameForUserSession(meeting, userSession);
                
                await _meetingDataProvider.UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);

                var updateUserSession = _mapper.Map<MeetingUserSessionDto>(userSession);

                updateUserSession.UserName = user.UserName;

                if (userSession.UserId == meeting.MeetingMasterUserId)
                {
                    updateUserSession.IsMeetingMaster = true;
                }
                
                meeting.UpdateUserSession(updateUserSession);

                meeting.MeetingTokenFromLiveKit = user.Issuer switch
                {
                    UserAccountIssuer.Guest => _liveKitServerUtilService.GenerateTokenForGuest(updateUserSession.GuestName, updateUserSession.GuestName, meeting.MeetingNumber),
                    UserAccountIssuer.Self or UserAccountIssuer.Wiltechs => _liveKitServerUtilService.GenerateTokenForJoinMeeting(user, meeting.MeetingNumber),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public async Task<UpdateMeetingResponse> UpdateMeetingAsync(UpdateMeetingCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.CheckCurrentLoggedInUser(cancellationToken).ConfigureAwait(false);
            
            var meeting = await _meetingDataProvider.GetMeetingByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);

            ValidateMeetingUpdateConditions(meeting, user);

            var updateMeeting = _mapper.Map(command, meeting);
            updateMeeting.Password = command.SecurityCode;

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

        public async Task<ChatRoomSettingAddOrUpdateEvent> AddOrUpdateChatRoomSettingAsync(AddOrUpdateChatRoomSettingCommand command, CancellationToken cancellationToken)
        {
            if (!_currentUser.Id.HasValue) throw new UnauthorizedAccessException();
            
            var roomSetting = await _meetingDataProvider.GetMeetingChatRoomSettingByMeetingIdAsync(_currentUser.Id.Value, command.MeetingId, cancellationToken).ConfigureAwait(false);

            if (roomSetting == null)
            {
                await _meetingDataProvider.AddMeetingChatRoomSettingAsync(new MeetingChatRoomSetting 
                {
                    UserId = _currentUser.Id.Value,
                    MeetingId = command.MeetingId,
                    VoiceId = command.VoiceId,
                    VoiceName = command.VoiceName,
                    Gender = command.Gender,
                    SelfLanguage = command.SelfLanguage,
                    ListeningLanguage = command.ListeningLanguage,
                }, true, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                Log.Information("Confirm whether to assign automatically roomSetting {@RoomSetting}", roomSetting);
                
                if (command.IsSystem && roomSetting.IsSystem != true)
                {
                    await AutoAssignAndUpdateVoiceIdAsync(roomSetting, command.MeetingId, cancellationToken);
                }
                else
                {
                    roomSetting.VoiceId = command.VoiceId;
                    roomSetting.VoiceName = command.VoiceName;
                    roomSetting.Gender = command.Gender;
                    roomSetting.SelfLanguage = command.SelfLanguage;
                    roomSetting.ListeningLanguage = command.ListeningLanguage;
                    roomSetting.IsSystem = command.IsSystem;

                    await _meetingDataProvider.UpdateMeetingChatRoomSettingAsync(roomSetting, true, cancellationToken).ConfigureAwait(false);
                }
            }

            return new ChatRoomSettingAddOrUpdateEvent();
        }

        private async Task AutoAssignAndUpdateVoiceIdAsync(MeetingChatRoomSetting roomSetting, Guid meetingId, CancellationToken cancellationToken)
        {
            var userSetting = await _meetingDataProvider.DistributeLanguageForMeetingUserAsync(meetingId, cancellationToken).ConfigureAwait(false);

            Log.Information("SugarTalk get userSetting from addOrUpdate roomSetting :{userSetting}", JsonConvert.SerializeObject(userSetting));

            var voiceId = roomSetting.ListeningLanguage switch
            {
                SpeechTargetLanguageType.Cantonese => (int)userSetting.CantoneseToneType,
                SpeechTargetLanguageType.Mandarin => (int)userSetting.MandarinToneType,
                SpeechTargetLanguageType.English => (int)userSetting.EnglishToneType,
                SpeechTargetLanguageType.Spanish => (int)userSetting.SpanishToneType,
                _ => 0
            };

            var stringVoiceId = voiceId.ToString();

            if (string.IsNullOrEmpty(stringVoiceId))
            {
                roomSetting.VoiceId = stringVoiceId;
                roomSetting.IsSystem = true;
            }
            
            await _meetingDataProvider.UpdateMeetingChatRoomSettingAsync(roomSetting, true, cancellationToken).ConfigureAwait(false);
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

        public async Task<MeetingSwitchEaResponse> UpdateMeetingChatResponseAsync(
            MeetingSwitchEaCommand switchEaCommand, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.CheckCurrentLoggedInUser(cancellationToken).ConfigureAwait(false);
            
            var meeting = await _meetingDataProvider.GetMeetingByIdAsync(switchEaCommand.Id, cancellationToken).ConfigureAwait(false);
            
            if (meeting.MeetingMasterUserId != user.Id) 
                throw new CannotUpdateMeetingWhenMasterUserIdMismatchException();

            Log.Information("Meeting master userId:{masterId}, current userId{currentUserId}", meeting.MeetingMasterUserId, _currentUser.Id.Value);
            
            meeting.IsActiveEa = switchEaCommand.IsActiveEa;
            
            await _meetingDataProvider.UpdateMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);

            return new MeetingSwitchEaResponse();
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
                LastJoinTime = _clock.Now.ToUnixTimeSeconds(),
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

        public async Task<AppointmentMeetingCanceledEvent> CancelAppointmentMeetingAsync(CancelAppointmentMeetingCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser?.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (user is null) throw new UnauthorizedAccessException();
            
            await _meetingDataProvider.CancelAppointmentMeetingAsync(command.MeetingId, cancellationToken).ConfigureAwait(false);

            return new AppointmentMeetingCanceledEvent();
        }

        public async Task<GetMeetingInviteInfoResponse> GetMeetingInviteInfoAsync(GetMeetingInviteInfoRequest request, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider
                .GetUserAccountAsync(_currentUser?.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (user is null) throw new UnauthorizedAccessException();

            var meeting = await _meetingDataProvider.GetMeetingByIdAsync(request.MeetingId, cancellationToken).ConfigureAwait(false);

            return new GetMeetingInviteInfoResponse
            {
                Data = new GetMeetingInviteInfoResponseData
                {
                    Sender = user.UserName,
                    Title = meeting.Title,
                    MeetingNumber = meeting.MeetingNumber,
                    Url = _meetingInfoSettings.InviteBaseUrl + $"/#/?meetingNumber={meeting.MeetingNumber}",
                    Password = meeting.Password
                }
            };
        }

        public async Task<MeetingRecordDeletedEvent> DeleteMeetingRecordAsync(DeleteMeetingRecordCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (user is null) throw new UnauthorizedAccessException();
            
            await _meetingDataProvider.DeleteMeetingRecordAsync(command.MeetingRecordIds, cancellationToken).ConfigureAwait(false);

            return new MeetingRecordDeletedEvent();
        }
        
        private void EnrichMeetingUserSessionForOutMeeting(MeetingUserSession userSession)
        {
            var lastQuitTimeBeforeChange =
                userSession.LastQuitTime ??
                userSession.LastJoinTime ??
                userSession.CreatedDate.ToUnixTimeSeconds();
            
            userSession.OnlineType = MeetingUserSessionOnlineType.OutMeeting;
            userSession.LastQuitTime = _clock.Now.ToUnixTimeSeconds();
            userSession.CumulativeTime = (userSession.CumulativeTime ?? 0) + (userSession.LastQuitTime - lastQuitTimeBeforeChange);
        }
        
        private void ValidateMeetingUpdateConditions(Meeting meeting, UserAccountDto currentUser)
        {
            if (meeting is null) 
                throw new MeetingNotFoundException();
            
            if (meeting.Status != MeetingStatus.Pending) 
                throw new CannotUpdateMeetingWhenStatusNotPendingException();
            
            Log.Information("Meeting master userId:{masterId}, current userId{currentUserId}", meeting.MeetingMasterUserId, _currentUser.Id.Value);
            
            if (meeting.MeetingMasterUserId != currentUser.Id) 
                throw new CannotUpdateMeetingWhenMasterUserIdMismatchException();
        }

        private void HandleGuestNameForUserSession(MeetingDto meeting, MeetingUserSession userSession)
        {
            var guestCount = meeting.UserSessions.Count(x => !string.IsNullOrEmpty(x.GuestName));

            var index = guestCount > 0 ? meeting.UserSessions
                    .Where(x => !string.IsNullOrEmpty(x.GuestName))
                    .Select(x => x.GuestName.Substring("Anonymity".Length))
                    .Select(x => int.TryParse(x, out int result) ? result : 0)
                    .Max() + 1 : 1;

            userSession.GuestName = $"Anonymity{index}";
        }
    }
}