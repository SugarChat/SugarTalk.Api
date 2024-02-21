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
    Task<GetMeetingRecordDetailsDto> GetMeetingRecordDetailsAsync(Guid recordId, CancellationToken cancellationToken);
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

    public async Task<GetMeetingRecordDetailsDto> GetMeetingRecordDetailsAsync(Guid recordId, CancellationToken cancellationToken)
    {
        var meetingRecordDetails = await _repository.Query<MeetingSpeakDetail>()
            .Where(x => x.MeetingRecordId == recordId).ToListAsync(cancellationToken: cancellationToken);

        var meetingInfos = await (
            from meetingRecord in _repository.Query<MeetingRecord>()
            join meeting in _repository.Query<Meeting>() on meetingRecord.MeetingId equals meeting.Id
            join meetingSummary in _repository.Query<MeetingSummary>() on meetingRecord.Id equals meetingSummary.RecordId
            where meetingRecord.Id == recordId
            select new GetMeetingRecordDetailsDto
            {
                Id = recordId,
                MeetingTitle = meeting.Title,
                MeetingNumber = meeting.MeetingNumber,
                MeetingStartDate = meeting.StartDate,
                MeetingEndDate = meeting.EndDate,
                Url = meetingRecord.Url,
                Summary = meetingSummary.Summary,
                MeetingRecordDetail = meetingRecordDetails.Select(x => _mapper.Map<MeetingRecordDetail>(x)).ToList()
            }
        ).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return meetingInfos;
    }
}