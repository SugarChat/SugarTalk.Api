using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarTalk.Core.Services.Exceptions
{
    public class CannotKickOutMeetingUserSessionException : Exception
    {
        public CannotKickOutMeetingUserSessionException() : base("The user cannot be kicked out of the meeting")
        {

        }
    }
}
