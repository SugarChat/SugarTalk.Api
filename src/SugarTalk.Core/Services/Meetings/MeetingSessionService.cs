using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingSessionService : IScopedDependency
    {
        Task<GetMeetingSessionResponse> GetMeetingSession(GetMeetingSessionRequest request,
            CancellationToken cancellationToken = default);

        Task ConnectUserToMeetingSession(UserAccount user, MeetingSessionDto meetingSession, string connectionId,
            bool? isMuted = null, CancellationToken cancellationToken = default);
        
        Task UpdateMeetingSession(MeetingSession meetingSession,
            CancellationToken cancellationToken = default);

        Task<MeetingSession> GenerateNewMeetingSession(Meeting meeting,
            CancellationToken cancellationToken);
    }
    
    public class MeetingSessionService : IMeetingSessionService
    {
        private readonly IMapper _mapper;
        private readonly IRepository _repository;
        private readonly IAccountService _accountService;
        private readonly IMeetingSessionDataProvider _meetingSessionDataProvider;
        
        public MeetingSessionService(IMapper mapper, 
            IRepository repository, IAccountService accountService, 
            IMeetingSessionDataProvider meetingSessionDataProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _accountService = accountService;
            _meetingSessionDataProvider = meetingSessionDataProvider;
        }
        
        public async Task<GetMeetingSessionResponse> GetMeetingSession(GetMeetingSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _accountService.GetCurrentLoggedInUser(cancellationToken).ConfigureAwait(false);

            if (user == null)
                throw new UnauthorizedAccessException();

            var meetingSession = await _meetingSessionDataProvider
                .GetMeetingSession(request.MeetingNumber, true, cancellationToken).ConfigureAwait(false);

            if (meetingSession != null && 
                meetingSession.UserSessions.Any() &&
                meetingSession.UserSessions.All(x => x.UserId != user.Uuid))
                throw new UnauthorizedAccessException();

            return new GetMeetingSessionResponse
            {
                Data = meetingSession
            };
        }

        public async Task UpdateMeetingSession(MeetingSession meetingSession, CancellationToken cancellationToken = default)
        {
            await _repository.UpdateAsync(meetingSession, cancellationToken).ConfigureAwait(false);
        }

        public async Task ConnectUserToMeetingSession(UserAccount user, MeetingSessionDto meetingSession, string connectionId, 
            bool? isMuted = null, CancellationToken cancellationToken = default)
        {
            var userSession = meetingSession.UserSessions
                .Where(x => x.UserId == user.Uuid)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => _mapper.Map<UserSession>(x))
                .FirstOrDefault();

            if (userSession == null)
            {
                userSession = GenerateNewUserSessionFromUser(user, meetingSession.Id, connectionId, isMuted ?? false);
                
                await _repository.InsertAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                meetingSession.AddUserSession(_mapper.Map<UserSessionDto>(userSession));
            }
            else
            {
                if (isMuted.HasValue)
                    userSession.IsMuted = isMuted.Value;
                
                userSession.ConnectionId = connectionId;
                
                await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                meetingSession.UpdateUserSession(_mapper.Map<UserSessionDto>(userSession));
            }
        }
        
        public async Task<MeetingSession> GenerateNewMeetingSession(Meeting meeting,
            CancellationToken cancellationToken)
        {
            var meetingSession = new MeetingSession
            {
                MeetingId = meeting.Id,
                MeetingType = meeting.MeetingType,
                MeetingNumber = meeting.MeetingNumber
            };

            await _repository.InsertAsync(meetingSession, cancellationToken).ConfigureAwait(false);

            return meetingSession;
        }
        
        private UserSession GenerateNewUserSessionFromUser(UserAccount user, Guid meetingSessionId, string connectionId, bool isMuted)
        {
            return new()
            {
                UserId = user.Uuid,
                UserName = user.UserName,
                UserPicture = user.Picture,
                MeetingSessionId = meetingSessionId,
                ConnectionId = connectionId,
                IsMuted = isMuted
            };
        }
    }
}