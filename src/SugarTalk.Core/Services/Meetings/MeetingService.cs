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
            UserAccountDto user, MeetingDto meeting, bool? isMuted = null, CancellationToken cancellationToken = default);
    }
    
    public class MeetingService: IMeetingService
    {
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly IAccountDataProvider _accountDataProvider;
        private readonly IMeetingDataProvider _meetingDataProvider;
        private readonly IUserSessionDataProvider _userSessionDataProvider;
        private readonly IAntMediaServerUtilService _antMediaServerUtilService;
        
        public MeetingService(
            IMapper mapper, 
            ICurrentUser currentUser,
            IMeetingDataProvider meetingDataProvider,
            IAccountDataProvider accountDataProvider,
            IUserSessionDataProvider userSessionDataProvider,
            IAntMediaServerUtilService antMediaServerUtilService)
        {
            _mapper = mapper;
            _currentUser = currentUser;
            _accountDataProvider = accountDataProvider;
            _meetingDataProvider = meetingDataProvider;
            _userSessionDataProvider = userSessionDataProvider;
            _antMediaServerUtilService = antMediaServerUtilService;
        }
        
        public async Task<ScheduleMeetingResponse> ScheduleMeeting(ScheduleMeetingCommand command, CancellationToken cancellationToken)
        {
            var postData = new CreateMeetingDto
            {
                MeetingNumber = GenerateMeetingNumber(),
                Mode = command.MeetingStreamMode.ToString().ToLower()
            };
            
            var response = await _antMediaServerUtilService.CreateMeetingAsync("LiveApp", postData, cancellationToken).ConfigureAwait(false);

            if (response == null) return new ScheduleMeetingResponse();
            
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
                .GetMeetingByMeetingNumberAsync(request.MeetingNumber, request.AppName, cancellationToken).ConfigureAwait(false);

            return new GetMeetingByNumberResponse
            {
                Data = _mapper.Map<MeetingDto>(response)
            };
        }

        public async Task<JoinMeetingResponse> JoinMeetingAsync(JoinMeetingCommand command, CancellationToken cancellationToken)
        {
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            var meeting = await GetMeetingAsync(command.MeetingNumber, cancellationToken).ConfigureAwait(false);

            await ConnectUserToMeetingAsync(user, meeting, command.IsMuted, cancellationToken).ConfigureAwait(false);
            
            return new JoinMeetingResponse
            {
                Data = meeting
            };
        }

        public async Task ConnectUserToMeetingAsync(
            UserAccountDto user, MeetingDto meeting, bool? isMuted = null, CancellationToken cancellationToken = default)
        {
            var userSession = meeting.UserSessions
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => _mapper.Map<MeetingUserSession>(x))
                .FirstOrDefault();

            if (userSession == null)
            {
                userSession = GenerateNewUserSessionFromUser(user, meeting.Id, isMuted ?? false);

                await _userSessionDataProvider.AddUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                meeting.AddUserSession(_mapper.Map<MeetingUserSessionDto>(userSession));
            }
            else
            {
                if (isMuted.HasValue)
                    userSession.IsMuted = isMuted.Value;

                await _userSessionDataProvider.UpdateUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
                
                meeting.UpdateUserSession(_mapper.Map<MeetingUserSessionDto>(userSession));
            }
        }

        private async Task<MeetingDto> GetMeetingAsync(
            string meetingNumber, CancellationToken cancellationToken, bool includeUserSessions = true)
        {
            var meeting = await _meetingDataProvider.GetMeetingByNumberAsync(meetingNumber, cancellationToken).ConfigureAwait(false);

            if (meeting == null) throw new MeetingNotFoundException();

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