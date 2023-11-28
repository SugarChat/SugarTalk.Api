using AutoMapper;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Dto.Meetings.User;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Core.Mapping
{
    public class MeetingMapping: Profile
    {
        public MeetingMapping()
        {
            CreateMap<Meeting, MeetingDto>().ReverseMap();
            CreateMap<ScheduleMeetingCommand, Meeting>();
            CreateMap<MeetingUserSession, MeetingUserSessionDto>()
                .ForMember(dest => dest.UserName, source => source.MapFrom(x => x.Name));
            CreateMap<MeetingUserSessionDto, MeetingUserSession>()
                .ForMember(dest => dest.Name, source => source.MapFrom(x => x.UserName));
            CreateMap<MeetingUserSessionStream, MeetingUserSessionStreamDto>().ReverseMap();
            CreateMap<UpdateMeetingCommand, Meeting>()
                .ForMember(dest => dest.StartDate, source => source.MapFrom(x => x.StartDate.ToUnixTimeSeconds()))
                .ForMember(dest => dest.EndDate, source => source.MapFrom(x => x.EndDate.ToUnixTimeSeconds()));
            CreateMap<ScheduleMeetingCommand, Meeting>()
                .ForMember(dest => dest.StartDate, source => source.MapFrom(x => x.StartDate.ToUnixTimeSeconds()))
                .ForMember(dest => dest.EndDate, source => source.MapFrom(x => x.EndDate.ToUnixTimeSeconds()));
            CreateMap<MeetingSpeech, MeetingSpeechDto>();
            CreateMap<MeetingUserSetting, MeetingUserSettingDto>();
        }
    }
}