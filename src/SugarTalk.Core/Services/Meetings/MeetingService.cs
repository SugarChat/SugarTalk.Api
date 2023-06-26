using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.AntMediaServer;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public partial interface IMeetingService : IScopedDependency
    {
        Task<MeetingScheduledEvent> ScheduleMeetingAsync(
            ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken);

        Task<GetMeetingByNumberResponse> GetMeetingByNumberAsync(
            GetMeetingByNumberRequest request, CancellationToken cancellationToken);

        Task<MeetingJoinedEvent> JoinMeetingAsync(
            JoinMeetingCommand command, CancellationToken cancellationToken);
        
        Task<MeetingOutedEvent> OutMeetingAsync(
            OutMeetingCommand command, CancellationToken cancellationToken);

        Task<MeetingEndedEvent> EndMeetingAsync(
            EndMeetingCommand command, CancellationToken cancellationToken);
    }
    
    public partial class MeetingService : IMeetingService
    {
        private const string appName = "LiveApp";

        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly IAccountDataProvider _accountDataProvider;
        private readonly IMeetingDataProvider _meetingDataProvider;
        private readonly IAntMediaServerUtilService _antMediaServerUtilService;
        
        public MeetingService(
            IMapper mapper, 
            ICurrentUser currentUser,
            IMeetingDataProvider meetingDataProvider,
            IAccountDataProvider accountDataProvider,
            IAntMediaServerUtilService antMediaServerUtilService)
        {
            _mapper = mapper;
            _currentUser = currentUser;
            _accountDataProvider = accountDataProvider;
            _meetingDataProvider = meetingDataProvider;
            _antMediaServerUtilService = antMediaServerUtilService;
        }
        
        public async Task<MeetingScheduledEvent> ScheduleMeetingAsync(ScheduleMeetingCommand command, CancellationToken cancellationToken)
        {
            var postData = new CreateMeetingDto
            {
                MeetingNumber = GenerateMeetingNumber(),
                Mode = command.MeetingStreamMode.ToString().ToLower(),
                StartDate = command.StartDate.ToUnixTimeSeconds(),
                EndDate = command.EndDate.ToUnixTimeSeconds()
            };
            
            var response = await _antMediaServerUtilService.CreateMeetingAsync(appName, postData, cancellationToken).ConfigureAwait(false);

            if (response == null) throw new CannotCreateMeetingException();
            
            var meeting = new Meeting
            {
                MeetingMasterUserId = _currentUser.Id.Value,
                MeetingStreamMode = command.MeetingStreamMode,
                MeetingNumber = response.MeetingNumber,
                OriginAddress = response.OriginAddress,
                StartDate = response.StartDate,
                EndDate = response.EndDate
            };
                
            await _meetingDataProvider
                .PersistMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);

            return new MeetingScheduledEvent
            {
                Meeting = _mapper.Map<MeetingDto>(meeting)
            };
        }
        
        public async Task<GetMeetingByNumberResponse> GetMeetingByNumberAsync(GetMeetingByNumberRequest request,
            CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider
                .GetMeetingAsync(request.MeetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting != null &&
                meeting.UserSessions.Any() &&
                meeting.UserSessions.All(x => x.UserId != _currentUser.Id))
                throw new UnauthorizedAccessException();

            return new GetMeetingByNumberResponse { Data = meeting };
        }

        public async Task<MeetingJoinedEvent> JoinMeetingAsync(JoinMeetingCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            var meeting = await _meetingDataProvider.GetMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            var response = await _antMediaServerUtilService
                .AddStreamToMeetingAsync(appName, meeting.MeetingNumber, command.StreamId, cancellationToken).ConfigureAwait(false);

            await ConnectUserToMeetingAsync(user, meeting, command.StreamId, command.StreamType, command.IsMuted, cancellationToken).ConfigureAwait(false);
            
            return new MeetingJoinedEvent
            {
                Meeting = meeting,
                Response = response
            };
        }
        
        public async Task<MeetingOutedEvent> OutMeetingAsync(OutMeetingCommand command, CancellationToken cancellationToken)
        {
            var userSession = await _meetingDataProvider
                .GetMeetingUserSessionByMeetingIdAsync(command.MeetingId, _currentUser.Id.Value, cancellationToken).ConfigureAwait(false);

            if (userSession == null) return new MeetingOutedEvent();

            var meeting = await _meetingDataProvider.GetMeetingByIdAsync(command.MeetingId, cancellationToken).ConfigureAwait(false);

            var response = await _antMediaServerUtilService
                .RemoveStreamFromMeetingAsync(appName, meeting.MeetingNumber, command.StreamId, cancellationToken).ConfigureAwait(false);

            await _meetingDataProvider
                .RemoveMeetingUserSessionStreamsAsync(new List<int> { userSession.Id }, cancellationToken).ConfigureAwait(false);
           
            await _meetingDataProvider
                .RemoveMeetingUserSessionsAsync(new List<MeetingUserSession> { userSession }, cancellationToken).ConfigureAwait(false);
            
            return new MeetingOutedEvent { Response = response };
        }
        
        public async Task<MeetingEndedEvent> EndMeetingAsync(EndMeetingCommand command, CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider.GetMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting.MeetingMasterUserId != _currentUser.Id) throw new CannotEndMeetingWhenUnauthorizedException();

            await _meetingDataProvider.RemoveMeetingUserSessionStreamsAsync(
                meeting.UserSessions.Select(x => x.Id).ToList(), cancellationToken).ConfigureAwait(false);

            await _meetingDataProvider.RemoveMeetingUserSessionsAsync(
                _mapper.Map<List<MeetingUserSession>>(meeting.UserSessions), cancellationToken).ConfigureAwait(false);

            await _meetingDataProvider.RemoveMeetingAsync(
                _mapper.Map<Meeting>(meeting), cancellationToken).ConfigureAwait(false);
            
            var response = await _antMediaServerUtilService
                .RemoveMeetingByMeetingNumberAsync(appName, meeting.MeetingNumber, cancellationToken)
                .ConfigureAwait(false);

            if (!response.Success) return new MeetingEndedEvent();

            return new MeetingEndedEvent
            {
                MeetingNumber = meeting.MeetingNumber,
                MeetingUserSessionIds = meeting.UserSessions.Select(x => x.Id).ToList()
            };
        }

        public async Task ConnectUserToMeetingAsync(
            UserAccountDto user, MeetingDto meeting, string streamId, MeetingStreamType streamType, bool? isMuted = null, CancellationToken cancellationToken = default)
        {
            await _meetingDataProvider
                .RemoveMeetingUserSessionsIfRequiredAsync(user.Id, meeting.Id, cancellationToken).ConfigureAwait(false);
            
            var userSession = meeting.UserSessions
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => _mapper.Map<MeetingUserSession>(x))
                .FirstOrDefault();

            if (userSession == null)
            {
                userSession = GenerateNewUserSessionFromUser(user, meeting.Id, isMuted ?? false);

                await _meetingDataProvider.AddMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                var updateUserSession = _mapper.Map<MeetingUserSessionDto>(userSession);
                
                var userSessionStream =
                    await AddMeetingUserSessionStreamIfRequiredAsync(updateUserSession.Id, streamId, streamType, cancellationToken).ConfigureAwait(false);

                updateUserSession.UserSessionStreams = new List<MeetingUserSessionStreamDto> { userSessionStream };

                updateUserSession.UserName = user.UserName;
                
                meeting.AddUserSession(updateUserSession);
            }
            else
            {
                if (isMuted.HasValue)
                    userSession.IsMuted = isMuted.Value;

                await _meetingDataProvider.UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                meeting.UpdateUserSession(_mapper.Map<MeetingUserSessionDto>(userSession));
            }
        }

        private async Task<MeetingUserSessionStreamDto> AddMeetingUserSessionStreamIfRequiredAsync(
            int userSessionId, string streamId, MeetingStreamType streamType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(streamId)) return null;
            
            var userSessionStream = new MeetingUserSessionStream
            {
                StreamId = streamId,
                StreamType = streamType,
                MeetingUserSessionId = userSessionId
            };

            await _meetingDataProvider.AddMeetingUserSessionStreamAsync(userSessionStream, cancellationToken).ConfigureAwait(false);

            return _mapper.Map<MeetingUserSessionStreamDto>(userSessionStream);
        }

        private string GenerateMeetingNumber()
        {
            var result = new StringBuilder();
            for (var i = 0; i < 5; i++)
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                result.Append(r.Next(0, 10));
            }
            return result.ToString();
        }
        
        private MeetingUserSession GenerateNewUserSessionFromUser(UserAccountDto user, Guid meetingId, bool isMuted)
        {
            return new MeetingUserSession
            {
                UserId = user.Id,
                IsMuted = isMuted,
                MeetingId = meetingId
            };
        }
    }
}