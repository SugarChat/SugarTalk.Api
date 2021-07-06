using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands;
using SugarTalk.Messages.Dtos;
using SugarTalk.Messages.Requests;

namespace SugarTalk.Core.Services
{
    public interface IMeetingService
    {
        Task<SugarTalkResponse<MeetingDto>> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken);

        Task<SugarTalkResponse<MeetingDto>> GetMeetingByNumber(GetMeetingByNumberRequest request,
            CancellationToken cancellationToken);
    }
    
    public class MeetingService: IMeetingService
    {
        private readonly IMapper _mapper;
        private readonly IMongoDbRepository _repository;
        private readonly IMeetingDataProvider _meetingDataProvider;
        
        public MeetingService(IMapper mapper, IMongoDbRepository repository, IMeetingDataProvider meetingDataProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _meetingDataProvider = meetingDataProvider;
        }
        
        public async Task<SugarTalkResponse<MeetingDto>> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken)
        {
            var meeting = _mapper.Map<Meeting>(scheduleMeetingCommand);

            meeting.MeetingNumber = GenerateMeetingNumber();
            
            await _repository.AddAsync(meeting, cancellationToken).ConfigureAwait(false);

            return new SugarTalkResponse<MeetingDto>
            {
                Data = _mapper.Map<MeetingDto>(meeting)
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