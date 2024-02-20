using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using LiveKit_CSharp.Services.Meeting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings
{
    public partial interface IMeetingService
    {
        Task<GetCurrentUserMeetingRecordResponse> GetCurrentUserMeetingRecordsAsync(GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken);
        
        Task<StorageMeetingRecordVideoResponse> StorageMeetingRecordVideoAsync(StorageMeetingRecordVideoCommand command, CancellationToken cancellationToken);

        Task<bool> StorageMeetingRecordVideoJobAsync(StorageMeetingRecordVideoCommand command, string token, CancellationToken cancellationToken);
    }

    public partial class MeetingService
    {
        public async Task<GetCurrentUserMeetingRecordResponse> GetCurrentUserMeetingRecordsAsync(GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken)
        {
            var (total, items) = await _meetingDataProvider.GetMeetingRecordsByUserIdAsync(_currentUser.Id, request, cancellationToken).ConfigureAwait(false);

            var response = new GetCurrentUserMeetingRecordResponse
            {
                Data = new GetCurrentUserMeetingRecordResponseDto
                {
                    Count = total,
                    Records = items
                }
            };

            return response;
        }
        
        public async Task<StorageMeetingRecordVideoResponse> StorageMeetingRecordVideoAsync(StorageMeetingRecordVideoCommand command, CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider.GetMeetingByIdAsync(command.MeetingId).ConfigureAwait(false);
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            var recordMeetingToken = _liveKitServerUtilService.GenerateTokenForRecordMeeting(user,meeting.MeetingNumber);
            var stopResponse = await _liveKitClient
                .StopEgressAsync(new StopEgressRequestDto { Token = recordMeetingToken, EgressId = command.EgressId },
                    cancellationToken).ConfigureAwait(false);
            if (stopResponse == null) throw new Exception();
            var startDate = DateTimeOffset.Now;

            RecurringJob.AddOrUpdate(nameof(ExecuteStorageMeetingRecordVideoDelayedJobAsync),
                () => ExecuteStorageMeetingRecordVideoDelayedJobAsync(startDate, command, recordMeetingToken,
                    cancellationToken), "*/5 * * * * ?");
            return new StorageMeetingRecordVideoResponse();
        }

        public async Task ExecuteStorageMeetingRecordVideoDelayedJobAsync(DateTimeOffset startDate,StorageMeetingRecordVideoCommand command,
            string token, CancellationToken cancellationToken)
        {  
            var now = DateTimeOffset.Now;
            if ((now - startDate).TotalMinutes > 5) RecurringJob.RemoveIfExists(nameof(ExecuteStorageMeetingRecordVideoDelayedJobAsync));

            var res = await StorageMeetingRecordVideoJobAsync(command, token, cancellationToken);
            if (res) RecurringJob.RemoveIfExists(nameof(ExecuteStorageMeetingRecordVideoDelayedJobAsync));
        }

        public async Task<bool> StorageMeetingRecordVideoJobAsync(StorageMeetingRecordVideoCommand command, string token, CancellationToken cancellationToken)
        {
            var meetingRecord = await _meetingDataProvider
                .GetMeetingRecordByMeetingRecordIdAsync(command.MeetingRecordId, cancellationToken)
                .ConfigureAwait(false);
            if (meetingRecord == null) return false;

            var getResponse = await _liveKitClient
                .GetEgressInfoListAsync(new GetEgressRequestDto { Token = token, EgressId = command.EgressId },
                    cancellationToken).ConfigureAwait(false);
            if (getResponse == null) return false;
            
            var egressItemDto =
                getResponse.EgressItems.FirstOrDefault(x => x.EgressId == command.EgressId && x.Status == "EGRESS_COMPLETE");
            if (egressItemDto == null) return false;

            meetingRecord.Url = egressItemDto.File.Location;
            meetingRecord.RecordType = MeetingRecordType.EndRecord;
            await _meetingDataProvider.UpdateMeetingRecordAsync(meetingRecord, cancellationToken).ConfigureAwait(false);
            return true;
        }
    }
}