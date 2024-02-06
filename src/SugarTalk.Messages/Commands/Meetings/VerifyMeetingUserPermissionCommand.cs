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
    public class VerifyMeetingUserPermissionCommand:ICommand
    {
        public Guid MeetingId { get; set; }
        
        public int UserId { get; set; }
    }

    public class VerifyMeetingUserPermissionResponse : SugarTalkResponse<MeetingUserSessionDto>
    {
    }
}
