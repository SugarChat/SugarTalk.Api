using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Foundation;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Extensions;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Messages.Dto;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public partial interface IMeetingDataProvider : IScopedDependency
    {
        Task<MeetingUserSession> GetMeetingUserSessionByMeetingIdAsync(Guid meetingId, Guid? meetingSubId, int? userId, MeetingUserSessionOnlineType? userSessionOnlineType, CancellationToken cancellationToken);
        
        Task<Meeting> GetMeetingByIdAsync(Guid? meetingId = null, string meetingNumber = null, CancellationToken cancellationToken = default);
        
        Task PersistMeetingAsync(Meeting meeting, CancellationToken cancellationToken);

        Task<MeetingDto> GetMeetingAsync(string meetingNumber = null, Guid? meetingId = null, CancellationToken cancellationToken = default, bool includeUserSessions = true);
        
        Task RemoveMeetingUserSessionsAsync(IEnumerable<MeetingUserSession> meetingUserSessions, CancellationToken cancellationToken = default);
        
        Task RemoveMeetingAsync(Meeting meeting, CancellationToken cancellationToken);
        
        Task UpdateMeetingAsync(Meeting meeting, CancellationToken cancellationToken);
        
        Task<List<MeetingUserSetting>> GetMeetingUserSettingsAsync(List<int> userIds, CancellationToken cancellationToken);
        
        Task<List<MeetingUserSetting>> GetMeetingUserSettingsAsync(Guid meetingId, CancellationToken cancellationToken);
        
        Task AddMeetingUserSettingAsync(MeetingUserSetting meetingUserSetting, CancellationToken cancellationToken);
        
        Task AddMeetingChatRoomSettingAsync(MeetingChatRoomSetting meetingChatRoomSetting, bool forSave = true, CancellationToken cancellationToken = default);

        Task UpdateMeetingUserSettingAsync(MeetingUserSetting meetingUserSetting, CancellationToken cancellationToken);
        
        Task UpdateMeetingChatRoomSettingAsync(MeetingChatRoomSetting chatRoomSetting, bool forSave = true, CancellationToken cancellationToken = default);

        Task<MeetingUserSetting> GetMeetingUserSettingByUserIdAsync(int userId, Guid meetingId, CancellationToken cancellationToken);
        
        Task<MeetingChatRoomSetting> GetMeetingChatRoomSettingByMeetingIdAsync(int userId, Guid meetingId, CancellationToken cancellationToken);

        Task<MeetingChatRoomSetting> GetMeetingChatRoomSettingByVoiceIdAsync(string voiceId, CancellationToken cancellationToken);
        
        Task CheckMeetingSecurityCodeAsync(Guid meetingId, string securityCode, CancellationToken cancellationToken);
        
        Task UpdateMeetingsAsync(List<Meeting> meetingList, CancellationToken cancellationToken);
        
        Task PersistMeetingRepeatRuleAsync(MeetingRepeatRule repeatRule, CancellationToken cancellationToken);
        
        Task PersistMeetingSubMeetingsAsync(List<MeetingSubMeeting> subMeetingList, CancellationToken cancellationToken);

        Task DeleteMeetingSubMeetingsAsync(Guid meetingId, CancellationToken cancellationToken);
        
        Task UpdateMeetingRepeatRuleAsync(Guid meetingId, MeetingRepeatType repeatType, CancellationToken cancellationToken);

        Task<(List<MeetingHistoryDto> MeetingHistoryList, int TotalCount)> GetMeetingHistoriesByUserIdAsync(
            int userId, string keyword, PageSetting pageSetting, CancellationToken cancellationToken);

        Task UpdateMeetingIfRequiredAsync(Guid meetingId, int userId, CancellationToken cancellationToken);
        
        Task PersistMeetingHistoryAsync(MeetingDto meeting, CancellationToken cancellationToken);
        
        Task<List<MeetingSubMeeting>> GetMeetingSubMeetingsAsync(Guid meetingId, CancellationToken cancellationToken);
        
        Task<(int Count, List<AppointmentMeetingDto> Records)> GetAppointmentMeetingsByUserIdAsync(GetAppointmentMeetingsRequest request, Guid? thirdPartyUserId, CancellationToken cancellationToken);
        
        Task MarkMeetingAsCompletedAsync(Meeting meeting, CancellationToken cancellationToken);

        Task UpdateUserSessionsAtMeetingEndAsync(Meeting meeting, List<MeetingUserSession> userSessions, CancellationToken cancellationToken);

        Task<MeetingUserSession> GetMeetingUserSessionByMeetingIdAndOnlineTypeAsync(Guid meetingId, int userId,
            CancellationToken cancellationToken);

        Task DeleteMeetingHistoryAsync(List<Guid> meetingHistoryIds, int userId, CancellationToken cancellationToken);
        
        Task CancelAppointmentMeetingAsync(Guid meetingId, CancellationToken cancellationToken);

        Task<List<MeetingUserSession>> GetMeetingUserSessionAsync(Guid meetingId, Guid? meetingSubId = null, int? userId = null, bool? coHost = null, MeetingUserSessionOnlineType? sessionOnlineType = null, CancellationToken cancellationToken = default);
        
        Task<List<Meeting>> GetAvailableRepeatMeetingAsync(CancellationToken cancellationToken);
        
        Task<List<MeetingSubMeeting>> GetMeetingSubMeetingsAsync(IEnumerable<Guid> meetingIds, CancellationToken cancellationToken);

        Task<List<Meeting>> GetAllAppointmentMeetingWithPendingAndInProgressAsync(CancellationToken cancellationToken);
        
        Task CheckUserKickedFromMeetingAsync(string meetingNumber, int userId, CancellationToken cancellationToken);

        Task<MeetingRepeatRule> GetMeetingRuleByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken);

        Task AddMeetingChatVoiceRecordAsync(List<MeetingChatVoiceRecord> meetingChatVoiceRecord, bool forSave = true, CancellationToken cancellationToken = default);
        
        Task<MeetingChatVoiceRecord> GetMeetingChatVoiceRecordAsync(Guid id, CancellationToken cancellationToken);

        Task AddMeetingParticipantAsync(List<MeetingParticipant> meetingParticipants, bool forSave = true, CancellationToken cancellationToken = default);

        Task<List<MeetingParticipant>> GetMeetingParticipantAsync(Guid meetingId, bool? isDesignatedHost = null, bool isUserAccount = false, CancellationToken cancellationToken = default);
        
        Task DeleteMeetingParticipantAsync(List<MeetingParticipant> meetingParticipants, bool forSave = true, CancellationToken cancellationToken = default);
    }
    
    public partial class MeetingDataProvider : IMeetingDataProvider
    {
        private readonly IClock _clock;
        private readonly IMapper _mapper;
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly IAccountDataProvider _accountDataProvider;

        public MeetingDataProvider(
            IClock clock, IMapper mapper, IRepository repository, IUnitOfWork unitOfWork, ICurrentUser currentUser, IAccountDataProvider accountDataProvider)
        {
            _clock = clock;
            _mapper = mapper;
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _accountDataProvider = accountDataProvider;
        }

        public async Task<MeetingUserSession> GetMeetingUserSessionByMeetingIdAsync(Guid meetingId, Guid? meetingSubId,
            int? userId, MeetingUserSessionOnlineType? userSessionOnlineType, CancellationToken cancellationToken)
        {
            var query = _repository
                .QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meetingId && x.UserId == userId);

            if (meetingSubId is not null)
            {
                query = query.Where(e => e.MeetingSubId == meetingSubId);
            }

            if (userSessionOnlineType.HasValue)
                query = query.Where(e => e.OnlineType == userSessionOnlineType);

            return await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Meeting> GetMeetingByIdAsync(Guid? meetingId = null, string meetingNumber = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.Query<Meeting>().AsNoTracking();

            if (meetingId.HasValue)
                query = query.Where(x => x.Id == meetingId);

            if (!string.IsNullOrEmpty(meetingNumber))
                query = query.Where(x => x.MeetingNumber == meetingNumber);
            
            return await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task PersistMeetingAsync(Meeting meeting, CancellationToken cancellationToken)
        {
            if (meeting is null) return;
            
            await _repository.InsertAsync(meeting, cancellationToken).ConfigureAwait(false);
        }
        
        public async Task<MeetingDto> GetMeetingAsync(
            string meetingNumber = null, Guid? meetingId = null, CancellationToken cancellationToken = default, bool includeUserSessions = true)
        {
            var query = _repository.QueryNoTracking<Meeting>();

            if(!string.IsNullOrEmpty(meetingNumber))
                query = query.Where(x => x.MeetingNumber == meetingNumber);

            if (meetingId.HasValue)
                query = query.Where(x => x.Id == meetingId);

            var meeting = await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            
            if (meeting == null) throw new MeetingNotFoundException();

            var updateMeeting = _mapper.Map<MeetingDto>(meeting);
            
            if (meeting.AppointmentType == MeetingAppointmentType.Appointment)
            {
                var subMeetings = await GetMeetingSubMeetingsAsync(meeting.Id, cancellationToken).ConfigureAwait(false);

                var subMeeting = subMeetings.FirstOrDefault(x => x.EndTime > _clock.Now.ToUnixTimeSeconds());
                
                if (subMeeting != null)
                {
                    updateMeeting.MeetingSubId = subMeeting.Id;
                    updateMeeting.StartDate = subMeeting.StartTime;
                    updateMeeting.EndDate = subMeeting.EndTime;
                }
            }

            if (!string.IsNullOrEmpty(meeting.SecurityCode))
            {
                updateMeeting.IsPasswordEnabled = true;
            }

            if (includeUserSessions)
            {
                var allUserSessions =
                    await GetUserSessionsByMeetingIdAsync(meeting.Id, updateMeeting.MeetingSubId, true, null, cancellationToken).ConfigureAwait(false);

                updateMeeting.UserSessions = await EnrichMeetingUserSessionsByOnlineAsync(allUserSessions, cancellationToken).ConfigureAwait(false);
            }

            var meetingRule = await GetMeetingRuleByMeetingIdAsync(updateMeeting.Id, cancellationToken).ConfigureAwait(false);

            if (meetingRule is not null)
            {
                updateMeeting.RepeatType = meetingRule.RepeatType;
            }

            var meetingRecord = await GetMeetingRecordAsync(meeting.Id, cancellationToken).ConfigureAwait(false);

            if (meetingRecord is not null)
            {
                updateMeeting.MeetingRecordId = meetingRecord.Id;
            }
            
            return updateMeeting;
        }

        private async Task<List<MeetingUserSessionDto>> EnrichMeetingUserSessionsByOnlineAsync(
            List<MeetingUserSessionDto> userSessions, CancellationToken cancellationToken)
        {
            var userIds = userSessions.Select(x => x.UserId);

            var userAccounts = await _repository
                .ToListAsync<UserAccount>(x => userIds.Contains(x.Id), cancellationToken).ConfigureAwait(false);

            var userAccountsDict = userAccounts.ToDictionary(x => x.Id, x => x);

            foreach (var session in userSessions)
            {
                if (!userAccountsDict.TryGetValue(session.UserId, out var user)) continue;

                session.UserName = user.UserName;
            }

            return userSessions;
        }
        
        public async Task RemoveMeetingUserSessionsAsync(
            IEnumerable<MeetingUserSession> meetingUserSessions, CancellationToken cancellationToken)
        {
            await _repository.DeleteAllAsync(meetingUserSessions, cancellationToken).ConfigureAwait(false);
        }

        public async Task RemoveMeetingAsync(Meeting meeting, CancellationToken cancellationToken)
        {
            await _repository.DeleteAsync(meeting, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateMeetingAsync(Meeting meeting, CancellationToken cancellationToken)
        {
            if (meeting is null) return;
            
            await _repository.UpdateAsync(meeting, cancellationToken).ConfigureAwait(false);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<MeetingUserSetting>> GetMeetingUserSettingsAsync(List<int> userIds, CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingUserSetting>()
                .Where(x => userIds.Contains(x.UserId))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<MeetingUserSetting>> GetMeetingUserSettingsAsync(Guid meetingId, CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingUserSetting>()
                .Where(x => x.MeetingId == meetingId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task AddMeetingUserSettingAsync(MeetingUserSetting meetingUserSetting, CancellationToken cancellationToken)
        {
            if (meetingUserSetting is null) return;

            await _repository.InsertAsync(meetingUserSetting, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddMeetingChatRoomSettingAsync(MeetingChatRoomSetting meetingChatRoomSetting, bool forSave = true, CancellationToken cancellationToken = default)
        {
            if (meetingChatRoomSetting is null) return;
           
            await _repository.InsertAsync(meetingChatRoomSetting, cancellationToken).ConfigureAwait(false);
            
            if (forSave) await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateMeetingUserSettingAsync(MeetingUserSetting meetingUserSetting, CancellationToken cancellationToken)
        {
            if (meetingUserSetting is null) return;
            
            await _repository.UpdateAsync(meetingUserSetting, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateMeetingChatRoomSettingAsync(MeetingChatRoomSetting meetingChatRoomSetting, bool forSave = true, CancellationToken cancellationToken = default)
        {
            if (meetingChatRoomSetting is null) return;
            
            await _repository.UpdateAsync(meetingChatRoomSetting, cancellationToken).ConfigureAwait(false);
            
            if (forSave) await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<MeetingUserSetting> GetMeetingUserSettingByUserIdAsync(int userId, Guid meetingId, CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingUserSetting>()
                .Where(x => x.UserId == userId && x.MeetingId == meetingId)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<MeetingChatRoomSetting> GetMeetingChatRoomSettingByMeetingIdAsync(int userId, Guid meetingId, CancellationToken cancellationToken)
        {
            return await _repository.Query<MeetingChatRoomSetting>()
                .Where(x => x.UserId == userId && x.MeetingId == meetingId)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<MeetingChatRoomSetting> GetMeetingChatRoomSettingByVoiceIdAsync(string voiceId, CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingChatRoomSetting>()
                .Where(x => x.VoiceId == voiceId)
                .OrderByDescending(x => x.LastModifiedDate)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task CheckMeetingSecurityCodeAsync(Guid meetingId, string securityCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(securityCode)) throw new MeetingSecurityCodeException();
            
            var code = securityCode.ToSha256();

            var isCorrect = await _repository.AnyAsync<Meeting>(x =>
                x.Id == meetingId && x.SecurityCode == code, cancellationToken).ConfigureAwait(false);

            if (!isCorrect) throw new MeetingSecurityCodeException();
        }

        public async Task UpdateMeetingsAsync(List<Meeting> meetingList, CancellationToken cancellationToken)
        {
            await _repository.UpdateAllAsync(meetingList, cancellationToken).ConfigureAwait(false);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task PersistMeetingRepeatRuleAsync(MeetingRepeatRule repeatRule, CancellationToken cancellationToken)
        {
            if (repeatRule is null) return;
            
            await _repository.InsertAsync(repeatRule, cancellationToken).ConfigureAwait(false);
        }

        public async Task PersistMeetingSubMeetingsAsync(List<MeetingSubMeeting> subMeetingList, CancellationToken cancellationToken)
        {
            if (subMeetingList is not { Count: > 0 }) return;
            
            await _repository.InsertAllAsync(subMeetingList, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteMeetingSubMeetingsAsync(Guid meetingId, CancellationToken cancellationToken)
        {
            var meetingSubMeetings = await _repository.Query<MeetingSubMeeting>()
                .Where(x => x.MeetingId == meetingId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            if (meetingSubMeetings is not { Count: > 0 }) return;

            meetingSubMeetings.ForEach(x => x.SubConferenceStatus = MeetingRecordSubConferenceStatus.NotExist);
        }

        public async Task UpdateMeetingRepeatRuleAsync(Guid meetingId, MeetingRepeatType repeatType, CancellationToken cancellationToken)
        {
            var meetingRepeatRule = await _repository.Query<MeetingRepeatRule>()
                .FirstOrDefaultAsync(x => x.MeetingId == meetingId, cancellationToken).ConfigureAwait(false);

            if (meetingRepeatRule is null) return;
            
            meetingRepeatRule.RepeatType = repeatType;
        }

        public async Task<(List<MeetingHistoryDto> MeetingHistoryList, int TotalCount)> GetMeetingHistoriesByUserIdAsync(
            int userId, string keyword, PageSetting pageSetting, CancellationToken cancellationToken)
        {
            var query = _repository.QueryNoTracking<MeetingHistory>()
                .OrderByDescending(x => x.CreatedDate)
                .Where(x => x.UserId == userId && !x.IsDeleted);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = from history in query
                    join meeting in _repository.QueryNoTracking<Meeting>() on history.MeetingId equals meeting.Id
                    join user in _repository.QueryNoTracking<UserAccount>() on meeting.CreatedBy equals user.Id
                    where meeting.Title.Contains(keyword) || meeting.MeetingNumber.Contains(keyword) || user.UserName.Contains(keyword)
                    select history;
            }

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            
            query = pageSetting != null
                ? query.Skip((pageSetting.Page - 1) * pageSetting.PageSize).Take(pageSetting.PageSize) : query;

            var meetingHistories = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            var meetingHistoryList = _mapper.Map<List<MeetingHistoryDto>>(meetingHistories);

            await EnrichMeetingHistoriesAsync(meetingHistoryList, cancellationToken).ConfigureAwait(false);

            Log.Information("Meeting history response:{@meetingHistoryList}, count: {totalCount}", JsonConvert.SerializeObject(meetingHistoryList), totalCount);

            return (meetingHistoryList, totalCount);
        }
        
        private async Task EnrichMeetingHistoriesAsync(List<MeetingHistoryDto> meetingHistoryList, CancellationToken cancellationToken)
        {
            var meetingIds = meetingHistoryList.Select(x => x.MeetingId).ToList();

            var meetingList = await _repository.QueryNoTracking<Meeting>()
                .Where(x => meetingIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, y => y, cancellationToken).ConfigureAwait(false);

            // todo : 找出UserSession所属的子会议
            var userSessions = await _repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => meetingIds.Contains(x.MeetingId))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            
            var userIds = userSessions.Select(x => x.UserId).Distinct().ToList();
            
            var userAccounts = await _accountDataProvider.GetUserAccountsAsync(userIds, cancellationToken).ConfigureAwait(false);
            
            var attendeesByMeeting = userSessions
                .GroupBy(x => (x.MeetingId, x.MeetingSubId))
                .ToDictionary(group => group.Key, group =>
                {
                    var attendees = group
                        .Select(x => GetAttendee(userAccounts, x)).ToList();
                    return attendees;
                });

            foreach (var meetingHistory in meetingHistoryList)
            {
                meetingList.TryGetValue(meetingHistory.MeetingId, out var meeting);
                attendeesByMeeting.TryGetValue((meetingHistory.MeetingId, meetingHistory.MeetingSubId), out var attendees);
                
                meetingHistory.MeetingNumber = meeting?.MeetingNumber;
                meetingHistory.Title = meeting?.Title;
                meetingHistory.TimeZone = meeting?.TimeZone;
                meetingHistory.StartDate = meeting?.StartDate ?? 0;
                meetingHistory.EndDate = meeting?.EndDate ?? 0;
                meetingHistory.attendees = attendees;
                meetingHistory.MeetingCreator = userAccounts.FirstOrDefault(x => x.Id == meeting?.MeetingMasterUserId)?.UserName;
                meetingHistory.AppointmentType = meeting?.AppointmentType;
            }
        }

        public async Task UpdateMeetingIfRequiredAsync(Guid meetingId, int userId, CancellationToken cancellationToken)
        {
            var meeting = await _repository.Query<Meeting>()
                .Where(x => x.Id == meetingId).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (meeting is null) return;

            meeting.Status = MeetingStatus.InProgress;
            
            //目前拿最近一次创建人进入会议时间
            if (meeting.MeetingMasterUserId == userId)
                meeting.CreatorJoinTime = _clock.Now.ToUnixTimeSeconds();

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task PersistMeetingHistoryAsync(MeetingDto meeting, CancellationToken cancellationToken)
        {
            if (meeting is null) return;

            var userSessions = await _repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meeting.Id)
                .Where(x => x.OnlineType != MeetingUserSessionOnlineType.KickOutMeeting)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var userIds = userSessions.Select(x => x.UserId).Distinct().ToList();
            
            var userAccounts = await _accountDataProvider.GetUserAccountsAsync(userIds, cancellationToken).ConfigureAwait(false);

            var meetingHistories = userAccounts.Select(user => new MeetingHistory
            {
                MeetingId = meeting.Id,
                MeetingSubId = meeting.MeetingSubId,
                CreatorJoinTime = meeting.CreatorJoinTime,
                UserId = user.Id,
                Duration = CalculateMeetingDuration(meeting.CreatorJoinTime, _clock.Now.ToUnixTimeSeconds())
            });

            await _repository.InsertAllAsync(meetingHistories, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<MeetingSubMeeting>> GetMeetingSubMeetingsAsync(Guid meetingId, CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingSubMeeting>()
                .Where(x => x.MeetingId == meetingId && x.SubConferenceStatus == MeetingRecordSubConferenceStatus.Default)
                .OrderBy(x => x.StartTime).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<(int Count, List<AppointmentMeetingDto> Records)> GetAppointmentMeetingsByUserIdAsync(GetAppointmentMeetingsRequest request, Guid? thirdPartyUserId, CancellationToken cancellationToken)
        {
            var maxQueryDate = _clock.Now.AddMonths(1).ToUnixTimeSeconds();
            var startOfDay = new DateTimeOffset(_clock.Now.Year, _clock.Now.Month, _clock.Now.Day, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
            
            Log.Information("GetAppointmentMeetingsByUserIdAsync maxQueryDate:{@maxQueryDate},startOfDay:{@startofDay}", maxQueryDate, startOfDay);

            var meetingIds = from staff in _repository.Query<RmStaff>()
                where staff.UserId == thirdPartyUserId
                join participant in _repository.Query<MeetingParticipant>() on staff.Id equals participant.StaffId
                select participant.MeetingId;
                    
            var query =
                from meeting in _repository.Query<Meeting>()
                where meeting.Status != MeetingStatus.Cancelled &&
                      (meeting.MeetingMasterUserId == _currentUser.Id || meeting.CreatedBy == _currentUser.Id || meetingIds.Contains(meeting.Id)) &&
                      meeting.AppointmentType == MeetingAppointmentType.Appointment
                join rules in _repository.Query<MeetingRepeatRule>() on meeting.Id equals rules.MeetingId
                join subMeetings in _repository.Query<MeetingSubMeeting>() on meeting.Id equals subMeetings.MeetingId
                    into subMeetingGroup
                from subMeeting in subMeetingGroup.DefaultIfEmpty()
                where (rules.RepeatType == MeetingRepeatType.None &&
                        meeting.StartDate >= startOfDay &&
                        meeting.EndDate <= maxQueryDate) ||
                       (subMeeting != null &&
                        subMeeting.StartTime >= startOfDay &&
                        subMeeting.EndTime <= maxQueryDate)
                select new AppointmentMeetingDto
                {
                    MeetingId = meeting.Id,
                    MeetingNumber = meeting.MeetingNumber,
                    StartDate = rules.RepeatType == MeetingRepeatType.None ? meeting.StartDate : subMeeting.StartTime,
                    EndDate = rules.RepeatType == MeetingRepeatType.None ? meeting.EndDate : subMeeting.EndTime,
                    Status = meeting.Status,
                    Title = meeting.Title,
                    Creator = meeting.CreatedBy,
                    TimeZone = meeting.TimeZone,
                    RepeatType = rules.RepeatType,
                    AppointmentType = meeting.AppointmentType,
                    CreatedDate = meeting.CreatedDate
                };

            var appointmentMeetingList = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            var filteredAppointmentMeetingList = appointmentMeetingList
                .GroupBy(meeting => meeting.MeetingId)
                .Select(g => g.MinBy(m => m.StartDate))
                .Where(x => x != null)
                .OrderBy(meeting => meeting.StartDate).ToList();

            var appointmentMeetingDtos = filteredAppointmentMeetingList
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize).ToList();
            
            return (filteredAppointmentMeetingList.Count, appointmentMeetingDtos);
        }

        public async Task MarkMeetingAsCompletedAsync(Meeting meeting, CancellationToken cancellationToken)
        {
            if (meeting.AppointmentType == MeetingAppointmentType.Quick)
            {
                meeting.EndDate = _clock.Now.ToUnixTimeSeconds();
            }
            
            meeting.Status = MeetingStatus.Completed;
            
            await _repository.UpdateAsync(meeting, cancellationToken).ConfigureAwait(false);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        
        public async Task UpdateUserSessionsAtMeetingEndAsync(Meeting meeting, List<MeetingUserSession> userSessions, CancellationToken cancellationToken)
        {
            userSessions.ForEach(x =>
            {
                if (x.LastQuitTime is not (null or 0)) return;

                x.CumulativeTime = (x.CumulativeTime ?? 0) + Convert.ToInt64(
                    (DateTimeOffset.FromUnixTimeSeconds(meeting.EndDate) -
                     DateTimeOffset.FromUnixTimeSeconds(meeting.StartDate)).TotalSeconds);
                x.LastQuitTime = meeting.EndDate;
                x.OnlineType = MeetingUserSessionOnlineType.OutMeeting;
            });

            await _repository.UpdateAllAsync(userSessions, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        
        private static long CalculateMeetingDuration(long startDate, long endDate)
        {
            if (endDate <= 0 || startDate <= 0 || endDate <= startDate) return 0;

            return endDate - startDate;
        }
        
        public async Task<MeetingUserSession> GetMeetingUserSessionByMeetingIdAndOnlineTypeAsync(Guid meetingId, int userId, CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meetingId)
                .Where(x => x.UserId == userId)
                .Where(x => x.OnlineType == MeetingUserSessionOnlineType.Online)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task DeleteMeetingHistoryAsync(List<Guid> meetingHistoryIds, int userId, CancellationToken cancellationToken)
        {
            var meetingHistories = await _repository.Query<MeetingHistory>()
                .Where(x => meetingHistoryIds.Contains(x.Id) && !x.IsDeleted)
                .Where(x => x.UserId == userId).ToListAsync(cancellationToken).ConfigureAwait(false);

            if (meetingHistories is not { Count: > 0 }) return;

            meetingHistories.ForEach(x => x.IsDeleted = true);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task CancelAppointmentMeetingAsync(Guid meetingId, CancellationToken cancellationToken)
        {
            var meeting = await _repository.Query<Meeting>()
                .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken).ConfigureAwait(false);

            if (meeting is null) throw new MeetingNotFoundException();

            if (meeting.Status != MeetingStatus.Pending) throw new CannotCancelAppointmentMeetingStatusException();

            meeting.Status = MeetingStatus.Cancelled;

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<MeetingUserSession>> GetMeetingUserSessionAsync(Guid meetingId, Guid? meetingSubId = null, int? userId = null, bool? coHost = null, MeetingUserSessionOnlineType? sessionOnlineType = null,  CancellationToken cancellationToken = default)
        {
            var query = _repository.QueryNoTracking<MeetingUserSession>().Where(x => x.MeetingId == meetingId);

            if (meetingSubId.HasValue) query = query.Where(x => x.MeetingSubId == meetingSubId.Value);

            if (userId.HasValue) query = query.Where(x => x.UserId == userId);

            if (sessionOnlineType.HasValue)
                query = query.Where(x => x.OnlineType == sessionOnlineType.Value);

            if (coHost.HasValue)
                query = query.Where(x => x.CoHost == coHost.Value);

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Meeting>> GetAvailableRepeatMeetingAsync(CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<Meeting>()
                .Where(x => x.AppointmentType == MeetingAppointmentType.Appointment)
                .Join(_repository.QueryNoTracking<MeetingRepeatRule>(), meeting => meeting.Id, rule => rule.MeetingId,
                    (meeting, rule) => new { meeting, rule })
                .Where(y => y.rule.RepeatType != MeetingRepeatType.None)
                .Select(x => x.meeting).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<MeetingSubMeeting>> GetMeetingSubMeetingsAsync(IEnumerable<Guid> meetingIds,
            CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingSubMeeting>()
                .Where(x => meetingIds.Contains(x.MeetingId))
                .Where(x => x.StartTime > _clock.Now.ToUnixTimeSeconds())
                .Where(x => x.SubConferenceStatus == MeetingRecordSubConferenceStatus.Default)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Meeting>> GetAllAppointmentMeetingWithPendingAndInProgressAsync(CancellationToken cancellationToken)
        {
            var appointmentMeetings = await _repository.Query<Meeting>()
                .Where(x => x.AppointmentType == MeetingAppointmentType.Appointment)
                .Where(x => x.Status == MeetingStatus.Pending || x.Status == MeetingStatus.InProgress)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            if (appointmentMeetings is not { Count: > 0 }) return new List<Meeting>();
            
            return await FilterAppointmentMeetingsWithoutAttendeesAsync(appointmentMeetings, cancellationToken).ConfigureAwait(false);
        }

        public async Task CheckUserKickedFromMeetingAsync(string meetingNumber, int userId, CancellationToken cancellationToken)
        {
            var isKicked = await _repository.QueryNoTracking<Meeting>()
                .Where(x => x.MeetingNumber == meetingNumber)
                .Join(_repository.QueryNoTracking<MeetingUserSession>(),
                    meeting => meeting.Id,
                    session => session.MeetingId,
                    (meeting, session) => new { meeting, session })
                .Where(x => x.session.UserId == userId && x.session.OnlineType == MeetingUserSessionOnlineType.KickOutMeeting)
                .AnyAsync(cancellationToken).ConfigureAwait(false);

            if (isKicked)
                throw new CannotJoinMeetingWhenKickedOutMeetingException(userId.ToString());
        }
        
        public async Task<MeetingRepeatRule> GetMeetingRuleByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingRepeatRule>()
                .FirstOrDefaultAsync(x => x.MeetingId == meetingId, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddMeetingChatVoiceRecordAsync(List<MeetingChatVoiceRecord> meetingChatVoiceRecord, bool forSave = true, CancellationToken cancellationToken = default)
        {
            if (meetingChatVoiceRecord is null) return;
            
            await _repository.InsertAllAsync(meetingChatVoiceRecord, cancellationToken).ConfigureAwait(false);
            
            if (forSave) await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<MeetingChatVoiceRecord> GetMeetingChatVoiceRecordAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _repository.Query<MeetingChatVoiceRecord>(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task AddMeetingParticipantAsync(List<MeetingParticipant> meetingParticipants, bool forSave = true, CancellationToken cancellationToken = default)
        {
            await _repository.InsertAllAsync(meetingParticipants, cancellationToken).ConfigureAwait(false);

            if (forSave)
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        
        public async Task<List<MeetingParticipant>> GetMeetingParticipantAsync(Guid meetingId, bool? isDesignatedHost = null, bool isUserAccount = false, CancellationToken cancellationToken = default)
        {
            var query = _repository.Query<MeetingParticipant>().Where(x => x.MeetingId == meetingId);

            if (isDesignatedHost.HasValue)
                query = query.Where(x => x.IsDesignatedHost == isDesignatedHost.Value);
            
            if (isUserAccount)
                query = from participant in query
                    join userAccount in _repository.Query<UserAccount>() on participant.StaffId.ToString() equals userAccount.ThirdPartyUserId
                    select new MeetingParticipant
                    {
                        Id = participant.Id,
                        MeetingId = participant.MeetingId,
                        StaffId = participant.StaffId,
                        IsDesignatedHost = participant.IsDesignatedHost,
                        UserId = userAccount.Id,
                        CreatedDate = participant.CreatedDate
                    };

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteMeetingParticipantAsync(List<MeetingParticipant> meetingParticipants, bool forSave = true, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteAllAsync(meetingParticipants, cancellationToken).ConfigureAwait(false);

            if (forSave)
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<MeetingRecord> GetMeetingRecordAsync(Guid meetingId, CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingRecord>(x => x.MeetingId == meetingId)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<List<Meeting>> FilterAppointmentMeetingsWithoutAttendeesAsync(List<Meeting> appointmentMeetings, CancellationToken cancellationToken)
        {
            var meetingIds = appointmentMeetings.Select(x => x.Id);

            // 过滤出席会议并在线的会议
            var filteredMeetingId = await _repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => meetingIds.Contains(x.MeetingId))
                .Where(x => x.Status == MeetingAttendeeStatus.Present && x.OnlineType == MeetingUserSessionOnlineType.Online)
                .Select(x => x.MeetingId).Distinct().ToListAsync(cancellationToken).ConfigureAwait(false);

            return appointmentMeetings.Where(x => !filteredMeetingId.Contains(x.Id)).ToList();
        }
        
        private static string GetAttendee(List<UserAccount> userAccounts, MeetingUserSession meetingUserSession)
        {
            var userAccount = userAccounts.FirstOrDefault(user => user.Id == meetingUserSession.UserId);

            if (userAccount == null)
                throw new Exception("UserAccount not found");

            return userAccount.Issuer switch
            {
                UserAccountIssuer.Self => userAccount.UserName,
                UserAccountIssuer.Guest => meetingUserSession.GuestName,
                UserAccountIssuer.Wiltechs => userAccount.UserName,
                _ => throw new Exception("Issuer inexistence")
            };
        }
    }
}