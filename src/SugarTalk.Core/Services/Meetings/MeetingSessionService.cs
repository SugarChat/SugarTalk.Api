using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Kurento.NET;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingSessionService
    {
        Task<SugarTalkResponse<MeetingSessionDto>> GetMeetingSession(GetMeetingSessionRequest request,
            CancellationToken cancellationToken = default);

        Task ConnectUserToMeetingSession(User user, MeetingSessionDto meetingSession, string connectionId,
            CancellationToken cancellationToken = default);
        
        Task UpdateMeetingSession(MeetingSession meetingSession,
            CancellationToken cancellationToken = default);

        Task UpdateUserSession(UserSession userSession, CancellationToken cancellationToken = default);
        
        Task UpdateUserSessionEndpoints(Guid userSessionId, WebRtcEndpoint endpoint,
            ConcurrentDictionary<string, WebRtcEndpoint> receivedEndPoints, CancellationToken cancellationToken = default);
        
        Task RemoveUserSession(string connectionId, CancellationToken cancellationToken = default);
        
        Task<MeetingSession> GenerateNewMeetingSession(Meeting meeting,
            CancellationToken cancellationToken);
    }
    
    public class MeetingSessionService : IMeetingSessionService
    {
        private readonly IMapper _mapper;
        private readonly KurentoClient _client;
        private readonly IUserService _userService;
        private readonly IMongoDbRepository _repository;
        private readonly IMeetingSessionDataProvider _meetingSessionDataProvider;
        
        public MeetingSessionService(IMapper mapper,  KurentoClient client, 
            IMongoDbRepository repository, IUserService userService, 
            IMeetingSessionDataProvider meetingSessionDataProvider)
        {
            _mapper = mapper;
            _client = client;
            _repository = repository;
            _userService = userService;
            _meetingSessionDataProvider = meetingSessionDataProvider;
        }
        
        public async Task<SugarTalkResponse<MeetingSessionDto>> GetMeetingSession(GetMeetingSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetCurrentLoggedInUser(cancellationToken).ConfigureAwait(false);

            if (user == null)
                throw new UnauthorizedAccessException();

            var meetingSession = await _meetingSessionDataProvider
                .GetMeetingSession(request.MeetingNumber, true, cancellationToken).ConfigureAwait(false);

            if (meetingSession != null && 
                meetingSession.UserSessions.Any() &&
                meetingSession.UserSessions.All(x => x.Value.UserId != user.Id))
                throw new UnauthorizedAccessException();

            return new SugarTalkResponse<MeetingSessionDto>
            {
                Data = meetingSession
            };
        }

        public async Task UpdateMeetingSession(MeetingSession meetingSession, CancellationToken cancellationToken = default)
        {
            await _repository.UpdateAsync(meetingSession, cancellationToken).ConfigureAwait(false);
        }

        public async Task ConnectUserToMeetingSession(User user, MeetingSessionDto meetingSession, string connectionId, 
            CancellationToken cancellationToken = default)
        {
            var userSession = meetingSession.AllUserSessions
                .Where(x => x.UserId == user.Id)
                .Select(x => _mapper.Map<UserSession>(x))
                .FirstOrDefault();

            if (userSession == null)
            {
                userSession = GenerateNewUserSessionFromUser(user, meetingSession.Id, connectionId);
                
                meetingSession.AddUserSession(_mapper.Map<UserSessionDto>(userSession));
                
                await _repository.AddAsync(userSession, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                userSession.ConnectionId = connectionId;

                meetingSession.UpdateUserSession(_mapper.Map<UserSessionDto>(userSession));
                
                await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
            }
        }
        
        public async Task UpdateUserSession(UserSession userSession, CancellationToken cancellationToken = default)
        {
            await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateUserSessionEndpoints(Guid userSessionId, WebRtcEndpoint endpoint,
            ConcurrentDictionary<string, WebRtcEndpoint> receivedEndPoints, CancellationToken cancellationToken = default)
        {
            var userSession = await _meetingSessionDataProvider.GetUserSessionById(userSessionId, cancellationToken)
                .ConfigureAwait(false);
            
            if (userSession == null) return;

            if (endpoint != null)
                userSession.WebRtcEndpointId = endpoint.id;
            if (receivedEndPoints != null && receivedEndPoints.Any())
                foreach (var (key, value) in receivedEndPoints)
                    userSession.ReceivedEndPointIds.TryAdd(key, value.id);
            
            await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
        }
        
        public async Task RemoveUserSession(string connectionId, CancellationToken cancellationToken = default)
        {
            var userSession = await _meetingSessionDataProvider
                .GetUserSessionByConnectionId(connectionId, cancellationToken).ConfigureAwait(false);

            await _repository.RemoveAsync(userSession, cancellationToken).ConfigureAwait(false);
        }
        
        public async Task<MeetingSession> GenerateNewMeetingSession(Meeting meeting,
            CancellationToken cancellationToken)
        {
            var pipeline = await _client.CreateAsync(new MediaPipeline());
            
            var meetingSession = new MeetingSession
            {
                PipelineId = pipeline.id,
                MeetingId = meeting.Id,
                MeetingType = meeting.MeetingType,
                MeetingNumber = meeting.MeetingNumber
            };

            await _repository.AddAsync(meetingSession, cancellationToken).ConfigureAwait(false);

            return meetingSession;
        }
        
        private UserSession GenerateNewUserSessionFromUser(User user, Guid meetingSessionId, string connectionId)
        {
            return new()
            {
                UserId = user.Id,
                UserName = user.DisplayName,
                UserPicture = user.Picture,
                MeetingSessionId = meetingSessionId,
                ConnectionId = connectionId
            };
        }
    }
}