using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;

namespace SugarTalk.Core.Services
{
    public interface IMeetingDataProvider
    {
        Task<Meeting> GetMeetingById(Guid meetingId, CancellationToken cancellationToken = default);
        Task<Meeting> GetMeetingByNumber(string meetingNumber, CancellationToken cancellationToken = default);
    }
    
    public class MeetingDataProvider : IMeetingDataProvider
    {
        private readonly IMongoDbRepository _repository;

        public MeetingDataProvider(IMongoDbRepository repository)
        {
            _repository = repository;
        }

        public async Task<Meeting> GetMeetingById(Guid meetingId, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<Meeting>().SingleOrDefaultAsync(x => x.Id == meetingId, cancellationToken)
                .ConfigureAwait(false);
        }
        
        public async Task<Meeting> GetMeetingByNumber(string meetingNumber, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<Meeting>().SingleOrDefaultAsync(x => x.MeetingNumber == meetingNumber, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}