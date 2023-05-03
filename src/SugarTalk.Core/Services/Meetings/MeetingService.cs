using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingService : IScopedDependency
    {
        Task<ScheduleMeetingResponse> ScheduleMeeting(
            ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken);

        Task<GetMeetingByNumberResponse> GetMeetingByNumberAsync(
            GetMeetingByNumberRequest request, CancellationToken cancellationToken);

        Task<JoinMeetingResponse> JoinMeetingAsync(
            JoinMeetingCommand command, CancellationToken cancellationToken);

        Task ConnectUserToMeetingAsync(
            UserAccountDto user, MeetingDto meeting, string connectionId, bool? isMuted = null, CancellationToken cancellationToken = default);
    }
    
    public class MeetingService: IMeetingService
    {
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly IAntMediaUtilService _antMediaUtilService;
        private readonly IAccountDataProvider _accountDataProvider;
        private readonly IMeetingDataProvider _meetingDataProvider;
        private readonly IUserSessionDataProvider _userSessionDataProvider;
        
        public MeetingService(
            IMapper mapper, 
            ICurrentUser currentUser,
            IMeetingDataProvider meetingDataProvider,
            IAccountDataProvider accountDataProvider,
            IAntMediaUtilService antMediaUtilService,
            IUserSessionDataProvider userSessionDataProvider)
        {
            _mapper = mapper;
            _currentUser = currentUser;
            _antMediaUtilService = antMediaUtilService;
            _accountDataProvider = accountDataProvider;
            _meetingDataProvider = meetingDataProvider;
            _userSessionDataProvider = userSessionDataProvider;
        }
        
        public async Task<ScheduleMeetingResponse> ScheduleMeeting(ScheduleMeetingCommand command, CancellationToken cancellationToken)
        {
            var postData = new CreateMeetingDto
            {
                MeetingNumber = GenerateMeetingNumber(),
                Mode = command.MeetingStreamMode == MeetingStreamMode.MCU ? "mcu" : "sfu"
            };
            
            var response = await _antMediaUtilService.CreateMeetingAsync(postData, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.MeetingNumber))
                await _meetingDataProvider
                    .PersistMeetingAsync(command.MeetingStreamMode, response, cancellationToken).ConfigureAwait(false);

            return new ScheduleMeetingResponse
            {
                Data = response
            };
        }
        
        public async Task<GetMeetingByNumberResponse> GetMeetingByNumberAsync(GetMeetingByNumberRequest request, CancellationToken cancellationToken)
        {
            var response = await _antMediaUtilService.GetMeetingByMeetingNumberAsync(request.MeetingNumber, cancellationToken).ConfigureAwait(false);

            return new GetMeetingByNumberResponse
            {
                Data = _mapper.Map<MeetingDto>(response)
            };
        }

        public async Task<JoinMeetingResponse> JoinMeetingAsync(JoinMeetingCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            var meeting = await GenerateMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            meeting.UserSessions =
                await _userSessionDataProvider.GetUserSessionsByMeetingId(meeting.Id, cancellationToken).ConfigureAwait(false);

            await ConnectUserToMeetingAsync(user, meeting, null, command.IsMuted, cancellationToken)
                .ConfigureAwait(false);
            
            return new JoinMeetingResponse
            {
                Data = meeting
            };
        }

        public async Task ConnectUserToMeetingAsync(
            UserAccountDto user, MeetingDto meeting, string roomStreamId, bool? isMuted = null, CancellationToken cancellationToken = default)
        {
            var userSession = meeting.UserSessions
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => _mapper.Map<UserSession>(x))
                .FirstOrDefault();

            if (userSession == null)
            {
                userSession = GenerateNewUserSessionFromUser(user, meeting.Id, roomStreamId, isMuted ?? false);

                meeting.AddUserSession(_mapper.Map<UserSessionDto>(userSession));

                await _userSessionDataProvider.AddUserSessionAsync(userSession, cancellationToken);
            }
            else
            {
                if (isMuted.HasValue)
                    userSession.IsMuted = isMuted.Value;
                
                userSession.RoomStreamId = roomStreamId;
                
                meeting.UpdateUserSession(_mapper.Map<UserSessionDto>(userSession));

                await _userSessionDataProvider.UpdateUserSessionAsync(userSession, cancellationToken);
            }
        }

        private async Task<MeetingDto> GenerateMeetingAsync(string meetingNumber, CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider.GetMeetingByNumberAsync(meetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting == null) throw new MeetingNotFoundException();

            var postData = await _antMediaUtilService
                .GetMeetingByMeetingNumberAsync(meetingNumber, cancellationToken).ConfigureAwait(false);

            return postData == null ? null : _mapper.Map<MeetingDto>(postData);
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
        
        private UserSession GenerateNewUserSessionFromUser(UserAccountDto user, Guid meetingId, string roomStreamId, bool isMuted)
        {
            return new()
            {
                UserId = user.Id,
                MeetingId = meetingId,
                RoomStreamId = roomStreamId,
                IsMuted = isMuted
            };
        }
    }
}