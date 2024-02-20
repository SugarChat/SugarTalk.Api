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
    public class StorageMeetingRecordVideoCommand : ICommand
    {
        public Guid MeetingId { get; set; }

        public string EgressId { get; set; }
        
        public Guid MeetingRecordId { get; set; }
    }
    public class StorageMeetingRecordVideoResponse : SugarTalkResponse
    {
    }
}
