using System;
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
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingService : IScopedDependency
    {
        Task<ScheduleMeetingResponse> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken);

        Task<JoinMeetingResponse> JoinMeetingAsync(JoinMeetingCommand joinMeetingCommand,
            CancellationToken cancellationToken);
        
        Task<GetMeetingByNumberResponse> GetMeetingByNumber(GetMeetingByNumberRequest request,
            CancellationToken cancellationToken);
    }
    
    public class MeetingService: IMeetingService
    {
        private readonly IMapper _mapper;
        private readonly IAccountService _userService;
        private readonly IMeetingUtilService _meetingUtilService;
        private readonly IMeetingDataProvider _meetingDataProvider;
        private readonly IMeetingSessionService _meetingSessionService;
        private readonly IMeetingSessionDataProvider _meetingSessionDataProvider;
        
        public MeetingService(
            IMapper mapper, 
            IMeetingDataProvider meetingDataProvider, 
            IAccountService userService, 
            IMeetingUtilService meetingUtilService,
            IMeetingSessionService meetingSessionService, 
            IMeetingSessionDataProvider meetingSessionDataProvider)
        {
            _mapper = mapper;
            _userService = userService;
            _meetingUtilService = meetingUtilService;
            _meetingDataProvider = meetingDataProvider;
            _meetingSessionService = meetingSessionService;
            _meetingSessionDataProvider = meetingSessionDataProvider;
        }
        
        public async Task<ScheduleMeetingResponse> ScheduleMeeting(ScheduleMeetingCommand command, CancellationToken cancellationToken)
        {
            var postData = new CreateMeetingDto
            {
                MeetingNumber = GenerateMeetingNumber(),
                Mode = command.MeetingMode == StreamMode.MCU ? "mcu" : "sfu"
            };
            
            var response = await _meetingUtilService.CreateMeetingAsync(postData, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.MeetingNumber))
                await _meetingDataProvider
                    .PersistMeetingAsync(response.Id, command.MeetingMode, response.MeetingNumber, response.OriginAdress, cancellationToken).ConfigureAwait(false);
            
            await _meetingSessionService.GenerateNewMeetingSession(
                response.Id, command.MeetingMode, response.MeetingNumber, cancellationToken).ConfigureAwait(false);

            response.MeetingType = command.MeetingType;
            
            return new ScheduleMeetingResponse
            {
                Data = response
            };
        }
        
        public async Task<JoinMeetingResponse> JoinMeetingAsync(JoinMeetingCommand joinMeetingCommand, CancellationToken cancellationToken)
        {
            var user = await _userService.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);

            var meetingSession = await _meetingSessionDataProvider
                .GetMeetingSession(joinMeetingCommand.MeetingNumber, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (meetingSession == null)
                throw new MeetingNotFoundException();

            await _meetingSessionService.ConnectUserToMeetingSession(
                    _mapper.Map<UserAccount>(user), meetingSession, null, joinMeetingCommand.IsMuted, cancellationToken).ConfigureAwait(false);

            return new JoinMeetingResponse
            {
                Data = meetingSession
            };
        }
        
        public async Task<GetMeetingByNumberResponse> GetMeetingByNumber(GetMeetingByNumberRequest request,
            CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider.GetMeetingByNumber(request.MeetingNumber, cancellationToken)
                .ConfigureAwait(false);

            if (meeting == null)
                throw new MeetingNotFoundException();
            
            return new GetMeetingByNumberResponse
            {
                Data = _mapper.Map<MeetingDto>(meeting)
            };
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
    }
}