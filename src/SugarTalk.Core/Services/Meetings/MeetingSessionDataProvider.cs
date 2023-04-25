using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Dtos.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingSessionDataProvider : IScopedDependency
    {
        Task<MeetingSession> GetMeetingSessionById(Guid id, CancellationToken cancellationToken = default);
        
        Task<MeetingSession> GetMeetingSessionByNumber(string meetingNumber,
            CancellationToken cancellationToken = default);

        Task<MeetingSessionDto> GetMeetingSession(string meetingNumber, bool includeUserSessions = true,
            CancellationToken cancellationToken = default);
    }
    
    public class MeetingSessionDataProvider : IMeetingSessionDataProvider
    {
        private readonly IMapper _mapper;
        private readonly IRepository _repository;
        private readonly IAntMediaClient _antMediaClient;
        private readonly IUserSessionDataProvider _userSessionDataProvider;
        
        public MeetingSessionDataProvider(IMapper mapper, IRepository repository, IAntMediaClient antMediaClient, IUserSessionDataProvider userSessionDataProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _antMediaClient = antMediaClient;
            _userSessionDataProvider = userSessionDataProvider;
        }

        public async Task<MeetingSession> GetMeetingSessionById(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<MeetingSession>()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MeetingSession> GetMeetingSessionByNumber(string meetingNumber, CancellationToken cancellationToken = default)
        {
            return await _repository.QueryNoTracking<MeetingSession>()
                .SingleOrDefaultAsync(x => x.MeetingNumber == meetingNumber, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<MeetingSessionDto> GetMeetingSession(string meetingNumber, bool includeUserSessions = true,
            CancellationToken cancellationToken = default)
        {
            var meeting = await GetMeetingSessionByNumber(meetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting == null)
                await _antMediaClient.GetAntMediaConferenceRoomAsync(meetingNumber, cancellationToken).ConfigureAwait(false);

            var meetingSession = _mapper.Map<MeetingSessionDto>(meeting);

            if (includeUserSessions)
            {
                meetingSession.UserSessions =
                    await _userSessionDataProvider.GetUserSessionsByMeetingSessionId(meetingSession.Id, cancellationToken).ConfigureAwait(false);
            }
            
            return meetingSession;
        }
    }
}