using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver.Linq;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingSessionDataProvider
    {
        Task<MeetingSession> GetMeetingSessionByNumber(string meetingNumber,
            CancellationToken cancellationToken = default);
    }
    
    public class MeetingSessionDataProvider : IMeetingSessionDataProvider
    {
        private readonly IMapper _mapper;
        private readonly IMongoDbRepository _repository;

        public MeetingSessionDataProvider(IMapper mapper, IMongoDbRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }
        
        public async Task<MeetingSession> GetMeetingSessionByNumber(string meetingNumber,
            CancellationToken cancellationToken = default)
        {
            return await _repository.Query<MeetingSession>()
                .SingleOrDefaultAsync(x => x.MeetingNumber == meetingNumber, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}