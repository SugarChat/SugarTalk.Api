using System;
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
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public partial interface IMeetingService : IScopedDependency
    {
        Task<ScheduleMeetingResponse> ScheduleMeetingAsync(
            ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken);

        Task<GetMeetingByNumberResponse> GetMeetingByNumberAsync(
            GetMeetingByNumberRequest request, CancellationToken cancellationToken);

        Task<JoinMeetingResponse> JoinMeetingAsync(
            JoinMeetingCommand command, CancellationToken cancellationToken);
        
        Task<OutMeetingResponse> OutMeetingAsync(
            OutMeetingCommand command, CancellationToken cancellationToken);

        Task ConnectUserToMeetingAsync(
            UserAccountDto user, MeetingDto meeting, bool? isMuted = null, CancellationToken cancellationToken = default);
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
        
        public async Task<ScheduleMeetingResponse> ScheduleMeetingAsync(ScheduleMeetingCommand command, CancellationToken cancellationToken)
        {
            var postData = new CreateMeetingDto
            {
                MeetingNumber = GenerateMeetingNumber(),
                Mode = command.MeetingStreamMode.ToString().ToLower()
            };
            
            var response = await _antMediaServerUtilService.CreateMeetingAsync(appName, postData, cancellationToken).ConfigureAwait(false);

            if (response == null) throw new MeetingCreatedException();
            
            var meeting = new Meeting
            {
                MeetingStreamMode = command.MeetingStreamMode,
                MeetingNumber = response.MeetingNumber,
                OriginAddress = response.OriginAddress,
                StartDate = response.StartDate,
                EndDate = response.EndDate
            };
                
            await _meetingDataProvider
                .PersistMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);

            return new ScheduleMeetingResponse
            {
                Data = response
            };
        }
        
        public async Task<GetMeetingByNumberResponse> GetMeetingByNumberAsync(GetMeetingByNumberRequest request, CancellationToken cancellationToken)
        {
            var response = await _antMediaServerUtilService
                .GetMeetingByMeetingNumberAsync(appName, request.MeetingNumber, cancellationToken).ConfigureAwait(false);

            return new GetMeetingByNumberResponse
            {
                Data = _mapper.Map<MeetingDto>(response)
            };
        }

        public async Task<JoinMeetingResponse> JoinMeetingAsync(JoinMeetingCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            var meeting = await _meetingDataProvider.GetMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            await ConnectUserToMeetingAsync(user, meeting, command.IsMuted, cancellationToken).ConfigureAwait(false);
            
            return new JoinMeetingResponse
            {
                Data = meeting
            };
        }
        
        public async Task<OutMeetingResponse> OutMeetingAsync(OutMeetingCommand command, CancellationToken cancellationToken)
        {
            var userSession = await _meetingDataProvider
                .GetMeetingUserSessionByIdAsync(command.MeetingUserSessionId, cancellationToken).ConfigureAwait(false);

            if (userSession == null) return null;
            
            await _meetingDataProvider.RemoveMeetingUserSession(userSession, cancellationToken).ConfigureAwait(false);
            
            return new OutMeetingResponse { Data = "success" };
        }

        public async Task ConnectUserToMeetingAsync(
            UserAccountDto user, MeetingDto meeting, bool? isMuted = null, CancellationToken cancellationToken = default)
        {
            await _meetingDataProvider
                .RemoveOtherMeetingUserSessionsAsync(user.Id, meeting.Id, cancellationToken).ConfigureAwait(false);
            
            var userSession = meeting.UserSessions
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => _mapper.Map<MeetingUserSession>(x))
                .FirstOrDefault();

            if (userSession == null)
            {
                userSession = GenerateNewUserSessionFromUser(user, meeting.Id, isMuted ?? false);

                await _meetingDataProvider.AddUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                meeting.AddUserSession(_mapper.Map<MeetingUserSessionDto>(userSession));
            }
            else
            {
                if (isMuted.HasValue)
                    userSession.IsMuted = isMuted.Value;

                await _meetingDataProvider.UpdateUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                meeting.UpdateUserSession(_mapper.Map<MeetingUserSessionDto>(userSession));
            }
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