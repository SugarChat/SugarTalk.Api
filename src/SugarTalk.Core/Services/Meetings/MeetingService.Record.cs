using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            var newestMeetingRecord = await _meetingDataProvider.GetNewestMeetingRecordByMeetingIdAsync(command.MeetingId, cancellationToken).ConfigureAwait(false);
            if (newestMeetingRecord == null) throw new MeetingRecordNotFoundException();
            if (newestMeetingRecord.RecordType == MeetingRecordType.EndRecord) throw new MeetingRecordNotOpenException();
            
            var context = _httpContextAccessor.HttpContext;
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            var jsonResponse = await _liveKitClient.StopEgressAsync(new StopEgressRequestDto {Token = token,EgressId = newestMeetingRecord.EgressId },cancellationToken).ConfigureAwait(false);
            if (jsonResponse == null) throw new MeetingRecordUrlNotFoundException();
            newestMeetingRecord.RecordType = MeetingRecordType.EndRecord;
            newestMeetingRecord.Url = jsonResponse.File.Location;
            await _meetingDataProvider.UpdateMeetingRecordAsync(newestMeetingRecord, cancellationToken).ConfigureAwait(false);
            
            return new StorageMeetingRecordVideoResponse();
        }
    }
}