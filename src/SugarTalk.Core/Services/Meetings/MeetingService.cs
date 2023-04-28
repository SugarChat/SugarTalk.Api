using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
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
            JoinMeetingCommand contextMessage, CancellationToken cancellationToken);
    }
    
    public class MeetingService: IMeetingService
    {
        private readonly IMapper _mapper;
        private readonly IAccountService _userService;
        private readonly IAntMediaUtilService _antMediaUtilService;
        private readonly IMeetingDataProvider _meetingDataProvider;
        
        public MeetingService(
            IMapper mapper, 
            IMeetingDataProvider meetingDataProvider, 
            IAccountService userService, 
            IAntMediaUtilService antMediaUtilService)
        {
            _mapper = mapper;
            _userService = userService;
            _antMediaUtilService = antMediaUtilService;
            _meetingDataProvider = meetingDataProvider;
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
                    .PersistMeetingAsync(command.MeetingStreamMode, response.MeetingNumber, response.OriginAddress, cancellationToken).ConfigureAwait(false);

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

        public async Task<JoinMeetingResponse> JoinMeetingAsync(JoinMeetingCommand contextMessage, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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