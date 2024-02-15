using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarTalk.Messages.Dto.Meetings
{
    public class VerifyMeetingUserPermissionDto
    {
        /// <summary>
        /// 是否为会议创建人
        /// </summary>
        public bool IsMeetingMaster { get; set; }
    }
}
