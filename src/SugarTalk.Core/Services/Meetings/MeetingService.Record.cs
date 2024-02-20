using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            var meetingRecord = await _meetingDataProvider.GetMeetingRecordByMeetingRecordIdAsync(command.MeetingRecordId, cancellationToken).ConfigureAwait(false);
            if (meetingRecord == null) throw new MeetingRecordNotFoundException();
            var meeting = await _meetingDataProvider.GetMeetingByIdAsync(command.MeetingId).ConfigureAwait(false);
            
            var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            var recordMeetingToken = _liveKitServerUtilService.GenerateTokenForRecordMeeting(user,meeting.MeetingNumber);

            var stopResponse = await _liveKitClient
                .StopEgressAsync(new StopEgressRequestDto { Token = recordMeetingToken, EgressId = command.EgressId },
                    cancellationToken).ConfigureAwait(false);
            if (stopResponse == null) throw new StopEgressResponseNotFoundException();

            var getResponse = await _liveKitClient
                .GetEgressInfoListAsync(new GetEgressRequestDto { Token = recordMeetingToken, EgressId = command.EgressId },
                    cancellationToken).ConfigureAwait(false);
            var egressItemDto =  getResponse.EgressItems.First(x => x.EgressId == command.EgressId);
            
            meetingRecord.RecordType = MeetingRecordType.EndRecord;
            meetingRecord.Url = egressItemDto.File.Location;
            await _meetingDataProvider.UpdateMeetingRecordAsync(meetingRecord, cancellationToken).ConfigureAwait(false);
            
            return new StorageMeetingRecordVideoResponse();
        }
    }
}