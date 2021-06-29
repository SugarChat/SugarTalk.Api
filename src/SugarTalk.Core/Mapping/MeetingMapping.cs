using AutoMapper;
using SugarTalk.Core.Entities;
using SugarTalk.Messages;

namespace SugarTalk.Core.Mapping
{
    public class MeetingMapping: Profile
    {
        public MeetingMapping()
        {
            CreateMap<ScheduleMeetingCommand, Meeting>();    
        }
        
    }
}