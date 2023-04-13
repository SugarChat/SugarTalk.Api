using AutoMapper;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Commands;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dtos;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Core.Mapping
{
    public class MeetingMapping: Profile
    {
        public MeetingMapping()
        {
            CreateMap<ScheduleMeetingCommand, Meeting>();
            CreateMap<Meeting, MeetingDto>();

            CreateMap<MeetingSession, MeetingSessionDto>();
        }
    }
}