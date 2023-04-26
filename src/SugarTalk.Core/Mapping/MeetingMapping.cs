using AutoMapper;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Core.Mapping
{
    public class MeetingMapping: Profile
    {
        public MeetingMapping()
        {
            CreateMap<ScheduleMeetingCommand, Meeting>();
            CreateMap<Meeting, MeetingDto>();
        }
    }
}