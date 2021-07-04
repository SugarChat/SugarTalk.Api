using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Messages;

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
        
        public Task<SugarTalkResponse<MeetingDto>> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}