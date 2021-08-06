using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Kurento.NET;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingSessionService
    {
        Task<SugarTalkResponse<MeetingSession>> GetMeetingSession(GetMeetingSessionRequest request,
            CancellationToken cancellationToken = default);

        Task UpdateMeetingSession(MeetingSession meetingSession,
            CancellationToken cancellationToken = default);
        
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
        
        public async Task<SugarTalkResponse<MeetingSession>> GetMeetingSession(GetMeetingSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetCurrentLoggedInUser(cancellationToken).ConfigureAwait(false);

            if (user == null)
                throw new UnauthorizedAccessException();

            var meetingSession = await _meetingSessionDataProvider
                .GetMeetingSessionByNumber(request.MeetingNumber, cancellationToken).ConfigureAwait(false);

            if (meetingSession != null && 
                meetingSession.UserSessions.Any() &&
                meetingSession.UserSessions.All(x => x.Value.UserId != user.Id))
                throw new UnauthorizedAccessException();

            if (meetingSession != null)
                meetingSession.Pipeline = _client.GetObjectById(meetingSession.PipelineId) as MediaPipeline;
            
            return new SugarTalkResponse<MeetingSession>
            {
                Data = meetingSession
            };
        }

        public async Task UpdateMeetingSession(MeetingSession meetingSession,
            CancellationToken cancellationToken = default)
        {
            await _repository.UpdateAsync(meetingSession, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MeetingSession> GenerateNewMeetingSession(Meeting meeting,
            CancellationToken cancellationToken)
        {
            var pipeline = await _client.CreateAsync(new MediaPipeline());
            
            return new MeetingSession
            {
                PipelineId = pipeline.id,
                MeetingId = meeting.Id,
                MeetingType = meeting.MeetingType,
                MeetingNumber = meeting.MeetingNumber,
                UserSessions = new ConcurrentDictionary<string, UserSession>()
            };
        }
    }
}