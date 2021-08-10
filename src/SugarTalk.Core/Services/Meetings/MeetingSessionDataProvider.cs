using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Kurento.NET;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingSessionDataProvider
    {
        Task<MeetingSession> GetMeetingSessionByNumber(string meetingNumber,
            CancellationToken cancellationToken = default);

        Task<MeetingSessionDto> GetMeetingSession(string meetingNumber, bool includeUserSessions = true,
            CancellationToken cancellationToken = default);
        
        Task<UserSession> GetUserSessionById(Guid id, CancellationToken cancellationToken = default);
        
        Task<UserSession> GetUserSessionByConnectionId(string connectionId,
            CancellationToken cancellationToken = default);
        
        Task<List<UserSessionDto>> GetUserSessions(Guid meetingSessionId, CancellationToken cancellationToken = default);
    }
    
    public class MeetingSessionDataProvider : IMeetingSessionDataProvider
    {
        private readonly IMapper _mapper;
        private readonly KurentoClient _client;
        private readonly IMongoDbRepository _repository;

        public MeetingSessionDataProvider(IMapper mapper, KurentoClient client, IMongoDbRepository repository)
        {
            _mapper = mapper;
            _client = client;
            _repository = repository;
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

            meetingSession.Pipeline = _client.GetObjectById(meetingSession.PipelineId) as MediaPipeline;

            if (includeUserSessions)
            {
                meetingSession.AllUserSessions =
                    await GetUserSessions(meetingSession.Id, cancellationToken).ConfigureAwait(false);
                meetingSession.AllUserSessions.ForEach(userSession =>
                    meetingSession.UserSessions.TryAdd(
                        !string.IsNullOrEmpty(userSession.ConnectionId)
                            ? userSession.ConnectionId
                            : userSession.Id.ToString(), userSession));
            }
            
            return meetingSession;
        }

        public async Task<UserSession> GetUserSessionById(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<UserSession>()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserSession> GetUserSessionByConnectionId(string connectionId, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<UserSession>()
                .FirstOrDefaultAsync(x => x.ConnectionId == connectionId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<UserSessionDto>> GetUserSessions(Guid meetingSessionId, CancellationToken cancellationToken = default)
        {
            var userSessions = await _repository.Query<UserSession>()
                .Where(x => x.MeetingSessionId == meetingSessionId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            
            var allUserSessions = _mapper.Map<List<UserSessionDto>>(userSessions);

            allUserSessions.ForEach(userSession =>
            {
                if (!string.IsNullOrEmpty(userSession.WebRtcEndpointId))
                    userSession.SendEndPoint = GetEndpointById(userSession.WebRtcEndpointId);
                if (userSession.ReceivedEndPointIds.Any())
                    foreach (var (key, value) in userSession.ReceivedEndPointIds)
                        userSession.ReceivedEndPoints.TryAdd(key, GetEndpointById(value));
            });
            
            return allUserSessions;
        }

        private WebRtcEndpoint GetEndpointById(string endpointId)
        {
            if (string.IsNullOrEmpty(endpointId)) return null;
            
            return _client.GetObjectById(endpointId) as WebRtcEndpoint;
        }
    }
}