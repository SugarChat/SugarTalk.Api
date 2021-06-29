using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IDatabaseProvider _databaseProvider;
        private readonly IMapper _mapper;

        public MeetingService(IDatabaseProvider databaseProvider, IMapper mapper)
        {
            _databaseProvider = databaseProvider;
            _mapper = mapper;
        }
        
        public Task<SugarTalkResponse<MeetingDto>> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}