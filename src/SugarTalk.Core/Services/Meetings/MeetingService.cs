using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingService
    {
        Task<SugarTalkResponse<MeetingDto>> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken);

        Task<SugarTalkResponse<MeetingSessionDto>> JoinMeeting(JoinMeetingCommand joinMeetingCommand,
            CancellationToken cancellationToken);
        
        Task<SugarTalkResponse<MeetingDto>> GetMeetingByNumber(GetMeetingByNumberRequest request,
            CancellationToken cancellationToken);
    }
    
    public class MeetingService: IMeetingService
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IMongoDbRepository _repository;
        private readonly IMeetingDataProvider _meetingDataProvider;

        private readonly IMeetingSessionService _meetingSessionService;
        private readonly IMeetingSessionDataProvider _meetingSessionDataProvider;
        
        public MeetingService(IMapper mapper, IMongoDbRepository repository,
            IMeetingDataProvider meetingDataProvider, IUserService userService, IMeetingSessionService meetingSessionService, IMeetingSessionDataProvider meetingSessionDataProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _userService = userService;
            _meetingDataProvider = meetingDataProvider;
            _meetingSessionService = meetingSessionService;
            _meetingSessionDataProvider = meetingSessionDataProvider;
        }
        
        public async Task<SugarTalkResponse<MeetingDto>> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken)
        {
            var meeting = _mapper.Map<Meeting>(scheduleMeetingCommand);

            meeting.MeetingNumber = GenerateMeetingNumber();

            await _repository.AddAsync(meeting, cancellationToken).ConfigureAwait(false);
            
            await _meetingSessionService.GenerateNewMeetingSession(meeting, cancellationToken)
                .ConfigureAwait(false);

            return new SugarTalkResponse<MeetingDto>
            {
                Data = _mapper.Map<MeetingDto>(meeting)
            };
        }
        
        public async Task<SugarTalkResponse<MeetingSessionDto>> JoinMeeting(JoinMeetingCommand joinMeetingCommand,
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

            return new SugarTalkResponse<MeetingSessionDto>
            {
                Data = meetingSession
            };
        }
        
        public async Task<SugarTalkResponse<MeetingDto>> GetMeetingByNumber(GetMeetingByNumberRequest request,
            CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider.GetMeetingByNumber(request.MeetingNumber, cancellationToken)
                .ConfigureAwait(false);

            if (meeting == null)
                throw new MeetingNotFoundException();
            
            return new SugarTalkResponse<MeetingDto>
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