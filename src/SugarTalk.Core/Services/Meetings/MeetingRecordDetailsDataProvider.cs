using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public interface IMeetingRecordDetailsDataProvider : IScopedDependency
{
    Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(Guid recordId, CancellationToken cancellationToken);
}

public class MeetingRecordDetailsDataProvider : IMeetingRecordDetailsDataProvider
{
    private readonly IMapper _mapper;
    private readonly IRepository _repository;

    public MeetingRecordDetailsDataProvider(IMapper mapper, IRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(Guid recordId, CancellationToken cancellationToken)
    {
        var meetingRecord = await _repository.Query<MeetingRecord>().FirstOrDefaultAsync(x => x.Id == recordId);

        var meetingInfo = await _repository.Query<Meeting>().FirstOrDefaultAsync(x => x.Id == meetingRecord.MeetingId);

        var meetingRecordDetails = await _repository.Query<MeetingSpeakDetail>().Where(x => x.MeetingRecordId == recordId).ToListAsync();

        var meetingSummary =  await _repository.Query<MeetingSummary>().FirstOrDefaultAsync(x => x.RecordId == recordId);

        return new GetMeetingRecordDetailsResponse
        {
            Data = new GetMeetingRecordDetailsDto()
            {
                Id = recordId,
                MeetingTitle = meetingInfo?.Title,
                MeetingNumber = meetingInfo?.MeetingNumber,
                MeetingStartDate = meetingInfo?.StartDate ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                MeetingEndDate =  meetingInfo?.EndDate ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Url = meetingRecord?.Url,
                Summary = meetingSummary?.Summary,
                MeetingRecordDetail = meetingRecordDetails.Select(x => _mapper.Map<MeetingRecordDetail>(x)).ToList()
            }
        };
    }
}