using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public partial interface IMeetingDataProvider : IScopedDependency
    {
        Task<MeetingUserSession> GetMeetingUserSessionByMeetingIdAsync(Guid meetingId, int userId, CancellationToken cancellationToken);
        
        Task<Meeting> GetMeetingByIdAsync(Guid meetingId, CancellationToken cancellationToken = default);
        
        Task PersistMeetingAsync(Meeting meeting, CancellationToken cancellationToken);

        Task<MeetingDto> GetMeetingAsync(string meetingNumber, CancellationToken cancellationToken, bool includeUserSessions = true);
        
        Task RemoveMeetingUserSessionsAsync(IEnumerable<MeetingUserSession> meetingUserSessions, CancellationToken cancellationToken);
        
        Task RemoveMeetingAsync(Meeting meeting, CancellationToken cancellationToken);
    }
    
    public partial class MeetingDataProvider : IMeetingDataProvider
    {
        private readonly IMapper _mapper;
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public MeetingDataProvider(IMapper mapper, IRepository repository, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _repository = repository;
            _unitOfWork = unitOfWork;
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
            return await _repository.Query<Meeting>()
                .SingleOrDefaultAsync(x => x.Id == meetingId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task PersistMeetingAsync(Meeting meeting, CancellationToken cancellationToken)
        {
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

            if (includeUserSessions)
            {
                updateMeeting.UserSessions =
                    await GetUserSessionsByMeetingIdAsync(meeting.Id, cancellationToken).ConfigureAwait(false);

                var userIds = updateMeeting.UserSessions.Select(x => x.UserId).ToList();

                var userAccounts = await _repository
                    .ToListAsync<UserAccount>(x => userIds.Contains(x.Id), cancellationToken)
                    .ConfigureAwait(false);

                updateMeeting.UserSessions.ForEach(userSession =>
                {
                    userSession.UserName = userAccounts
                        .Where(x => x.Id == userSession.UserId)
                        .Select(x => x.UserName).FirstOrDefault();
                });
            }
            
            return updateMeeting;
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
    }
}