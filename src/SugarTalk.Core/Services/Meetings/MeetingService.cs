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
            UserAccountDto user, MeetingDto meeting, List<string> streamIds, bool? isMuted = null, CancellationToken cancellationToken = default);
    }
    
    public class MeetingService: IMeetingService
    {
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly IAntMediaUtilServer _antMediaUtilServer;
        private readonly IAccountDataProvider _accountDataProvider;
        private readonly IMeetingDataProvider _meetingDataProvider;
        private readonly IUserSessionDataProvider _userSessionDataProvider;
        
        public MeetingService(
            IMapper mapper, 
            ICurrentUser currentUser,
            IAntMediaUtilServer antMediaUtilService,
            IMeetingDataProvider meetingDataProvider,
            IAccountDataProvider accountDataProvider,
            IUserSessionDataProvider userSessionDataProvider)
        {
            _mapper = mapper;
            _currentUser = currentUser;
            _antMediaUtilServer = antMediaUtilService;
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
            
            var response = await _antMediaUtilServer.CreateMeetingAsync(postData, command.AppName, cancellationToken).ConfigureAwait(false);

            var meeting = new Meeting
            {
                MeetingStreamMode = command.MeetingStreamMode,
                MeetingNumber = response.MeetingNumber,
                OriginAddress = response.OriginAddress,
                StartDate = response.StartDate,
                EndDate = response.EndDate
            };
            
            if (!string.IsNullOrEmpty(response.MeetingNumber))
                await _meetingDataProvider
                    .PersistMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);

            return new ScheduleMeetingResponse
            {
                Data = response
            };
        }
        
        public async Task<GetMeetingByNumberResponse> GetMeetingByNumberAsync(GetMeetingByNumberRequest request, CancellationToken cancellationToken)
        {
            var response = await _antMediaUtilServer
                .GetMeetingByMeetingNumberAsync(request.MeetingNumber, request.AppName, cancellationToken).ConfigureAwait(false);

            return new GetMeetingByNumberResponse
            {
                Data = _mapper.Map<MeetingDto>(response)
            };
        }

        public async Task<JoinMeetingResponse> JoinMeetingAsync(JoinMeetingCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            var meeting = await GetMeetingAsync(command.MeetingNumber, command.AppName, cancellationToken).ConfigureAwait(false);

            await ConnectUserToMeetingAsync(user, meeting, command.StreamIds, command.IsMuted, cancellationToken).ConfigureAwait(false);
            
            return new JoinMeetingResponse
            {
                Data = meeting
            };
        }

        public async Task ConnectUserToMeetingAsync(
            UserAccountDto user, MeetingDto meeting, List<string> streamIds, bool? isMuted = null, CancellationToken cancellationToken = default)
        {
            var userSession = meeting.UserSessions
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => _mapper.Map<MeetingUserSession>(x))
                .FirstOrDefault();

            if (userSession == null)
            {
                userSession = GenerateNewUserSessionFromUser(user, meeting.Id, isMuted ?? false);

                meeting.AddUserSession(_mapper.Map<MeetingUserSessionDto>(userSession));
                
                await _userSessionDataProvider.AddUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (isMuted.HasValue)
                    userSession.IsMuted = isMuted.Value;

                await _userSessionDataProvider.RemoveUserSessionStreamsAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                await _userSessionDataProvider.UpdateUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                meeting.UpdateUserSession(_mapper.Map<MeetingUserSessionDto>(userSession));
            }
            
            await _userSessionDataProvider.AddUserSessionStreamAsync(userSession, streamIds, cancellationToken).ConfigureAwait(false);
        }

        private async Task<MeetingDto> GetMeetingAsync(
            string meetingNumber, string appName, CancellationToken cancellationToken, bool includeUserSessions = true)
        {
            var meeting = await _meetingDataProvider.GetMeetingByNumberAsync(meetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting == null) throw new MeetingNotFoundException();

            var response = await _antMediaUtilServer
                .GetMeetingByMeetingNumberAsync(meetingNumber, appName, cancellationToken).ConfigureAwait(false);

            if (response != null)
                meeting.StreamList = response.RoomStreamList;

            if (includeUserSessions)
            {
                meeting.UserSessions =
                    await _userSessionDataProvider.GetUserSessionsByMeetingIdAsync(meeting.Id, cancellationToken).ConfigureAwait(false);
            }
            
            return meeting;
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
                MeetingId = meetingId,
                IsMuted = isMuted
            };
        }
    }
}