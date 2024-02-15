using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarTalk.Messages.Commands.Meetings
{
    public class StorageMeetingVideoByMeetingNumberCommand : ICommand
    {
        public string MeetingNumber { get; set; }
    }
    public class StorageMeetingVideoByMeetingNumberResponse : SugarTalkResponse<StorageMeetingVideoByMeetingNumberResponseData>
    {

    }
    public class StorageMeetingVideoByMeetingNumberResponseData
    {
        public StorageMeetingVideoDto StorageVideoDto { get; set; }
    }
}
