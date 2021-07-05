using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands;
using SugarTalk.Messages.Dtos;

namespace SugarTalk.Core.Services
{
    public interface IMeetingService
    {
        Task<SugarTalkResponse<MeetingDto>> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken);
    }
    
    public class MeetingService: IMeetingService
    {
        private readonly IMapper _mapper;
        private readonly IMongoDbRepository _repository;
        
        public MeetingService(IMapper mapper, IMongoDbRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }
        
        public async Task<SugarTalkResponse<MeetingDto>> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken)
        {
            var meeting = _mapper.Map<Meeting>(scheduleMeetingCommand);
            
            await _repository.AddAsync(meeting, cancellationToken).ConfigureAwait(false);

            return new SugarTalkResponse<MeetingDto>
            {
                Data = _mapper.Map<MeetingDto>(meeting)
            };
        }
    }
}