using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingDataProvider : IScopedDependency
    {
        Task<Meeting> GetMeetingById(Guid meetingId, CancellationToken cancellationToken = default);
        
        Task<MeetingDto> GetMeetingByNumberAsync(string meetingNumber, CancellationToken cancellationToken);
        
        Task PersistMeetingAsync(
            Meeting meeting, CancellationToken cancellationToken);

        Task<MeetingDto> GetMeetingAsync(string meetingNumber, CancellationToken cancellationToken, bool includeUserSessions = true);
    }
    
    public class MeetingDataProvider : IMeetingDataProvider
    {
        private readonly IMapper _mapper;
        private readonly IRepository _repository;
        private readonly IMeetingUserSessionDataProvider _meetingUserSessionDataProvider;

        public MeetingDataProvider(IMapper mapper, IRepository repository, IMeetingUserSessionDataProvider meetingUserSessionDataProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _meetingUserSessionDataProvider = meetingUserSessionDataProvider;
        }

        public async Task<Meeting> GetMeetingById(Guid meetingId, CancellationToken cancellationToken = default)
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
            var meeting = await _repository.Query<Meeting>()
                .SingleOrDefaultAsync(x => x.MeetingNumber == meetingNumber, cancellationToken)
                .ConfigureAwait(false);

            if (meeting == null) throw new MeetingNotFoundException();

            var updateMeeting = _mapper.Map<MeetingDto>(meeting);

            if (includeUserSessions)
            {
                updateMeeting.UserSessions =
                    await _meetingUserSessionDataProvider.GetUserSessionsByMeetingIdAsync(meeting.Id, cancellationToken).ConfigureAwait(false);
            }
            
            return updateMeeting;
        }
    }
}