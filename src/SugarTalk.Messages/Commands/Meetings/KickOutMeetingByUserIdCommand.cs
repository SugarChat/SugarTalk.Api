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
    public class KickOutMeetingByUserIdCommand: ICommand
    {
        public string MeetingNumber { get; set; }
        public Guid MeetingId { get; set; }
        public int KickOutUserId { get; set; }

        public int MasterUserId { get; set; }
    }

    public class KickOutMeetingByUserIdResponse : SugarTalkResponse<MeetingDto>
    {
    }
}
