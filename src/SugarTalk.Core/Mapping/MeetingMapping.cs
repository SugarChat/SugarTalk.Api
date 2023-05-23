using AutoMapper;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Core.Mapping
{
    public class MeetingMapping: Profile
    {
        public MeetingMapping()
        {
            CreateMap<Meeting, MeetingDto>().ReverseMap();
            CreateMap<ScheduleMeetingCommand, Meeting>();
            CreateMap<MeetingUserSession, MeetingUserSessionDto>().ReverseMap();
        }
    }
}