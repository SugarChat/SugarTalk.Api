using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver.Linq;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages.Dtos.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingSessionDataProvider
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
        private readonly IMongoDbRepository _repository;
        private readonly IUserSessionDataProvider _userSessionDataProvider;
        
        public MeetingSessionDataProvider(IMapper mapper, IMongoDbRepository repository, IUserSessionDataProvider userSessionDataProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _userSessionDataProvider = userSessionDataProvider;
        }

        public async Task<MeetingSession> GetMeetingSessionById(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<MeetingSession>()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MeetingSession> GetMeetingSessionByNumber(string meetingNumber,
            CancellationToken cancellationToken = default)
        {
            return await _repository.Query<MeetingSession>()
                .SingleOrDefaultAsync(x => x.MeetingNumber == meetingNumber, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<MeetingSessionDto> GetMeetingSession(string meetingNumber, bool includeUserSessions = true,
            CancellationToken cancellationToken = default)
        {
            var meeting = await GetMeetingSessionByNumber(meetingNumber, cancellationToken).ConfigureAwait(false);

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