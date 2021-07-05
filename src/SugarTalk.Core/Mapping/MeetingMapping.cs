using AutoMapper;
using SugarTalk.Core.Entities;
using SugarTalk.Messages.Commands;
using SugarTalk.Messages.Dtos;

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