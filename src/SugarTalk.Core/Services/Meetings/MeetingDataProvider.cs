using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Extensions;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Messages.Dto;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Services.Meetings
{
    public partial interface IMeetingDataProvider : IScopedDependency
    {
        Task<MeetingUserSession> GetMeetingUserSessionByMeetingIdAsync(Guid meetingId, int userId, CancellationToken cancellationToken);
        
        Task<Meeting> GetMeetingByIdAsync(Guid meetingId, CancellationToken cancellationToken = default);
        
        Task PersistMeetingAsync(Meeting meeting, CancellationToken cancellationToken);

        Task<MeetingDto> GetMeetingAsync(string meetingNumber, CancellationToken cancellationToken = default, bool includeUserSessions = true);
        
        Task RemoveMeetingUserSessionsAsync(IEnumerable<MeetingUserSession> meetingUserSessions, CancellationToken cancellationToken = default);
        
        Task RemoveMeetingAsync(Meeting meeting, CancellationToken cancellationToken);
        
        Task UpdateMeetingAsync(Meeting meeting, CancellationToken cancellationToken);
        
        Task<List<MeetingUserSetting>> GetMeetingUserSettingsAsync(List<int> userIds, CancellationToken cancellationToken);
        
        Task<List<MeetingUserSetting>> GetMeetingUserSettingsAsync(Guid meetingId, CancellationToken cancellationToken);
        
        Task AddMeetingUserSettingAsync(MeetingUserSetting meetingUserSetting, CancellationToken cancellationToken);
        
        Task UpdateMeetingUserSettingAsync(MeetingUserSetting meetingUserSetting, CancellationToken cancellationToken);
        
        Task<MeetingUserSetting> GetMeetingUserSettingByUserIdAsync(int userId, Guid meetingId, CancellationToken cancellationToken);
        
        Task CheckMeetingSecurityCodeAsync(Guid meetingId, string securityCode, CancellationToken cancellationToken);
        
        Task UpdateMeetingsAsync(List<Meeting> meetingList, CancellationToken cancellationToken);
        
        Task PersistMeetingRepeatRuleAsync(MeetingRepeatRule repeatRule, CancellationToken cancellationToken);
        
        Task PersistMeetingSubMeetingsAsync(List<MeetingSubMeeting> subMeetingList, CancellationToken cancellationToken);

        Task DeleteMeetingSubMeetingsAsync(Guid meetingId, CancellationToken cancellationToken);
        
        Task UpdateMeetingRepeatRuleAsync(Guid meetingId, MeetingRepeatType repeatType, CancellationToken cancellationToken);

        Task<(List<MeetingHistoryDto> MeetingHistoryList, int TotalCount)> GetMeetingHistoriesByUserIdAsync(
            Guid userId, PageSetting pageSetting, CancellationToken cancellationToken);

        Task UpdateMeetingIfRequiredAsync(Guid meetingId, int userId, CancellationToken cancellationToken);
        
        Task PersistMeetingHistoryAsync(MeetingDto meeting, CancellationToken cancellationToken);
    }
    
    public partial class MeetingDataProvider : IMeetingDataProvider
    {
        private readonly IClock _clock;
        private readonly IMapper _mapper;
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly IAccountDataProvider _accountDataProvider;

        public MeetingDataProvider(IClock clock,
            IMapper mapper, IRepository repository, IUnitOfWork unitOfWork, IAccountDataProvider accountDataProvider, ICurrentUser currentUser)
        {
            _clock = clock;
            _mapper = mapper;
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _accountDataProvider = accountDataProvider;
        }
        
        public async Task<MeetingUserSession> GetMeetingUserSessionByMeetingIdAsync(Guid meetingId, int userId, CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meetingId)
                .Where(x => x.UserId == userId)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Meeting> GetMeetingByIdAsync(Guid meetingId, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<Meeting>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == meetingId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task PersistMeetingAsync(Meeting meeting, CancellationToken cancellationToken)
        {
            if (meeting is null) return;
            
            await _repository.InsertAsync(meeting, cancellationToken).ConfigureAwait(false);
        }
        
        public async Task<MeetingDto> GetMeetingAsync(
            string meetingNumber, CancellationToken cancellationToken, bool includeUserSessions = true)
        {
            var meeting = await _repository.QueryNoTracking<Meeting>()
                .SingleOrDefaultAsync(x => x.MeetingNumber == meetingNumber, cancellationToken)
                .ConfigureAwait(false);

            if (meeting == null) throw new MeetingNotFoundException();

            var updateMeeting = _mapper.Map<MeetingDto>(meeting);

            if (!string.IsNullOrEmpty(meeting.SecurityCode))
            {
                updateMeeting.IsPasswordEnabled = true;
            }

            if (includeUserSessions)
            {
                updateMeeting.UserSessions =
                    await GetUserSessionsByMeetingIdAsync(meeting.Id, cancellationToken).ConfigureAwait(false);

                await EnrichMeetingUserSessionsAsync(updateMeeting.UserSessions, cancellationToken).ConfigureAwait(false);
            }
            
            return updateMeeting;
        }

        private async Task EnrichMeetingUserSessionsAsync(
            List<MeetingUserSessionDto> userSessions, CancellationToken cancellationToken)
        {
            var userIds = userSessions.Select(x => x.UserId);

            var userAccounts = await _repository
                .ToListAsync<UserAccount>(x => userIds.Contains(x.Id), cancellationToken).ConfigureAwait(false);

            userSessions.ForEach(userSession =>
            {
                userSession.UserName = userAccounts
                    .Where(x => x.Id == userSession.UserId)
                    .Select(x => x.UserName).FirstOrDefault();
            });
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

        public async Task UpdateMeetingUserSettingAsync(MeetingUserSetting meetingUserSetting, CancellationToken cancellationToken)
        {
            if (meetingUserSetting is null) return;
            
            await _repository.UpdateAsync(meetingUserSetting, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MeetingUserSetting> GetMeetingUserSettingByUserIdAsync(int userId, Guid meetingId, CancellationToken cancellationToken)
        {
            return await _repository.QueryNoTracking<MeetingUserSetting>()
                .Where(x => x.UserId == userId && x.MeetingId == meetingId)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task CheckMeetingSecurityCodeAsync(Guid meetingId, string securityCode, CancellationToken cancellationToken)
        {
            var code = securityCode.ToSha256();

            var isCorrect = await _repository.AnyAsync<Meeting>(x =>
                x.Id == meetingId && x.SecurityCode == code, cancellationToken).ConfigureAwait(false);

            if (!isCorrect) throw new MeetingSecurityCodeNotMatchException();
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
            Guid userId, PageSetting pageSetting, CancellationToken cancellationToken)
        {
            var query = _repository.QueryNoTracking<MeetingHistory>()
                .Where(x => x.UserEntityId == userId && x.IsDeleted);

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            
            query = pageSetting != null
                ? query.Skip((pageSetting.Page - 1) * pageSetting.PageSize).Take(pageSetting.PageSize) : query;
            
            var meetingHistories = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            var meetingHistoryList = _mapper.Map<List<MeetingHistoryDto>>(meetingHistories);
            
            await EnrichMeetingHistoriesAsync(meetingHistoryList, cancellationToken).ConfigureAwait(false);

            return (meetingHistoryList, totalCount);
        }
        
        private async Task EnrichMeetingHistoriesAsync(List<MeetingHistoryDto> meetingHistoryList, CancellationToken cancellationToken)
        {
            var meetingIds = meetingHistoryList.Select(x => x.MeetingId).ToList();

            var meetingList = await _repository.QueryNoTracking<Meeting>()
                .Where(x => meetingIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, y => y, cancellationToken).ConfigureAwait(false);

            var userSessions = await _repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => meetingIds.Contains(x.MeetingId))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            
            var userIds = userSessions.Select(x => x.UserId).Distinct().ToList();
            
            var userAccounts = await _accountDataProvider.GetUserAccountsAsync(userIds, cancellationToken).ConfigureAwait(false);
            
            var attendeesByMeetingId = userSessions.GroupBy(x => x.MeetingId)
                .ToDictionary(group => group.Key, group =>
                {
                    var attendees = group
                        .Select(x => userAccounts.FirstOrDefault(user => user.Id == x.UserId)?.UserName).ToList();
                    return attendees;
                });

            foreach (var meetingHistory in meetingHistoryList)
            {
                meetingList.TryGetValue(meetingHistory.MeetingId, out var meeting);
                attendeesByMeetingId.TryGetValue(meetingHistory.MeetingId, out var attendees);
                
                meetingHistory.MeetingNumber = meeting?.MeetingNumber;
                meetingHistory.Title = meeting?.Title;
                meetingHistory.TimeZone = meeting?.TimeZone;
                meetingHistory.StartDate = meeting?.StartDate ?? 0;
                meetingHistory.EndDate = meeting?.EndDate ?? 0;
                meetingHistory.attendees = attendees;
                meetingHistory.MeetingCreator = userAccounts.FirstOrDefault(x => x.Id == meeting?.MeetingMasterUserId)?.UserName;
            }
        }

        public async Task UpdateMeetingIfRequiredAsync(Guid meetingId, int userId, CancellationToken cancellationToken)
        {
            var meeting = await _repository.Query<Meeting>().Where(x=>x.Id == meetingId).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (meeting is null) return;

            meeting.Status = MeetingStatus.InProgress;
            
            if (meeting.MeetingMasterUserId == userId)
                meeting.CreatorJoinTime = _clock.Now.ToUnixTimeSeconds();

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task PersistMeetingHistoryAsync(MeetingDto meeting, CancellationToken cancellationToken)
        {
            if (meeting is null) return;

            var userSessions = await _repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meeting.Id).ToListAsync(cancellationToken).ConfigureAwait(false);

            var userIds = userSessions.Select(x => x.UserId).Distinct().ToList();
            
            var userAccounts = await _accountDataProvider.GetUserAccountsAsync(userIds, cancellationToken).ConfigureAwait(false);

            var meetingHistories = userAccounts.Select(user => new MeetingHistory
                {
                    Id = Guid.NewGuid(),
                    MeetingId = meeting.Id,
                    CreatorJoinTime = meeting.CreatorJoinTime,
                    UserEntityId = user.Uuid,
                    Duration = CalculateMeetingDuration(meeting.CreatorJoinTime, _clock.Now.ToUnixTimeSeconds())
                }).ToList();

            await _repository.InsertAllAsync(meetingHistories, cancellationToken).ConfigureAwait(false);
        }

        private static long CalculateMeetingDuration(long startDate, long endDate)
        {
            if (endDate <= 0 || startDate <= 0 || endDate <= startDate) return 0;
            
            return endDate - startDate;
        }
    }
}