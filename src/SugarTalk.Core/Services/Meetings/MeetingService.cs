using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingService : IScopedDependency
    {
        Task<ScheduleMeetingResponse> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken);

        Task<JoinMeetingResponse> JoinMeeting(JoinMeetingCommand joinMeetingCommand,
            CancellationToken cancellationToken);
        
        Task<GetMeetingByNumberResponse> GetMeetingByNumber(GetMeetingByNumberRequest request,
            CancellationToken cancellationToken);
    }
    
    public class MeetingService: IMeetingService
    {
        private readonly IMapper _mapper;
        private readonly IAccountService _userService;
        private readonly IRepository _repository;
        private readonly IMeetingDataProvider _meetingDataProvider;

        private readonly IMeetingSessionService _meetingSessionService;
        private readonly IMeetingSessionDataProvider _meetingSessionDataProvider;
        
        public MeetingService(IMapper mapper, IRepository repository,
            IMeetingDataProvider meetingDataProvider, IAccountService userService, IMeetingSessionService meetingSessionService, IMeetingSessionDataProvider meetingSessionDataProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _userService = userService;
            _meetingDataProvider = meetingDataProvider;
            _meetingSessionService = meetingSessionService;
            _meetingSessionDataProvider = meetingSessionDataProvider;
        }
        
        public async Task<ScheduleMeetingResponse> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken)
        {
            var meeting = _mapper.Map<Meeting>(scheduleMeetingCommand);

            meeting.MeetingNumber = GenerateMeetingNumber();

            await _repository.InsertAsync(meeting, cancellationToken).ConfigureAwait(false);
            
            await _meetingSessionService.GenerateNewMeetingSession(meeting, cancellationToken)
                .ConfigureAwait(false);

            return new ScheduleMeetingResponse
            {
                Data = _mapper.Map<MeetingDto>(meeting)
            };
        }
        
        public async Task<JoinMeetingResponse> JoinMeeting(JoinMeetingCommand joinMeetingCommand,
            CancellationToken cancellationToken)
        {
            var user = await _userService.GetCurrentLoggedInUser(cancellationToken).ConfigureAwait(false);

            if (user == null)
                throw new UnauthorizedAccessException();

            var meetingSession = await _meetingSessionDataProvider
                .GetMeetingSession(joinMeetingCommand.MeetingNumber, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (meetingSession == null)
                throw new MeetingNotFoundException();

            await _meetingSessionService.ConnectUserToMeetingSession(user, meetingSession, null, joinMeetingCommand.IsMuted, cancellationToken)
                .ConfigureAwait(false);

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