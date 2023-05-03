using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Services.Meetings
{
    public interface IMeetingDataProvider : IScopedDependency
    {
        Task<Meeting> GetMeetingById(Guid meetingId, CancellationToken cancellationToken = default);
        
        Task<MeetingDto> GetMeetingByNumberAsync(string meetingNumber, CancellationToken cancellationToken);
        
        Task PersistMeetingAsync(
            MeetingStreamMode meetingMode, CreateMeetingResponseDto meetingResponseData, CancellationToken cancellationToken);
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
        
        public async Task<MeetingDto> GetMeetingByNumberAsync(string meetingNumber, CancellationToken cancellationToken)
        {
            var meeting = await _repository.Query<Meeting>()
                .SingleOrDefaultAsync(x => x.MeetingNumber == meetingNumber, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<MeetingDto>(meeting);
        }

        public async Task PersistMeetingAsync(MeetingStreamMode meetingMode, CreateMeetingResponseDto meetingResponseData, CancellationToken cancellationToken)
        {
            var meeting = new Meeting
            {
                MeetingStreamMode = meetingMode,
                MeetingNumber = meetingResponseData.MeetingNumber,
                OriginAddress = meetingResponseData.OriginAddress,
                StartDate = meetingResponseData.StartDate,
                EndDate = meetingResponseData.EndDate
            };

            await _repository.InsertAsync(meeting, cancellationToken).ConfigureAwait(false);
        }
    }
}