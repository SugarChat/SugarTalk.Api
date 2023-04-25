using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingDataProvider : IScopedDependency
    {
        Task<Meeting> GetMeetingById(Guid meetingId, CancellationToken cancellationToken = default);
        
        Task<Meeting> GetMeetingByNumber(string meetingNumber, CancellationToken cancellationToken = default);
        
        Task PersistMeetingAsync(
            Guid meetingId, StreamMode meetingMode, string meetingNumber, string originAdress, CancellationToken cancellationToken);
    }
    
    public class MeetingDataProvider : IMeetingDataProvider
    {
        private readonly IMapper _mapper;
        private readonly IRepository _repository;

        public MeetingDataProvider(IMapper mapper, IRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<Meeting> GetMeetingById(Guid meetingId, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<Meeting>()
                .SingleOrDefaultAsync(x => x.Id == meetingId, cancellationToken)
                .ConfigureAwait(false);
        }
        
        public async Task<Meeting> GetMeetingByNumber(string meetingNumber, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<Meeting>()
                .SingleOrDefaultAsync(x => x.MeetingNumber == meetingNumber, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task PersistMeetingAsync(Guid meetingId, StreamMode meetingMode, string meetingNumber, string originAdress, CancellationToken cancellationToken)
        {
            var meeting = new Meeting
            {
                Id = meetingId,
                MeetingNumber = meetingNumber,
                MeetingMode = meetingMode,
                OriginAdress = originAdress
            };

            await _repository.InsertAsync(meeting, cancellationToken).ConfigureAwait(false);
        }
    }
}