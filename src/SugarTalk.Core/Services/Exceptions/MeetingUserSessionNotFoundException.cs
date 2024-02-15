using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarTalk.Core.Services.Exceptions
{
    public class MeetingUserSessionNotFoundException : Exception
    {
        public MeetingUserSessionNotFoundException() : base("Meeting not found")
        {
        }
    }

}
