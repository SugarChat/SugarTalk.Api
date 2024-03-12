using AutoMapper;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Dto.Meetings.Summary;
using SugarTalk.Messages.Dto.Meetings.User;

namespace SugarTalk.Core.Mapping;

public class MeetingMapping: Profile
{
    public MeetingMapping()
    {
        CreateMap<Meeting, MeetingDto>().ReverseMap();
        CreateMap<ScheduleMeetingCommand, Meeting>();
        CreateMap<MeetingUserSession, MeetingUserSessionDto>();
        CreateMap<MeetingUserSession, VerifyMeetingUserPermissionDto>();
        CreateMap<MeetingUserSessionDto, MeetingUserSession>();
        CreateMap<UpdateMeetingCommand, Meeting>()
            .ForMember(dest => dest.StartDate, source => source.MapFrom(x => x.StartDate.ToUnixTimeSeconds()))
            .ForMember(dest => dest.EndDate, source => source.MapFrom(x => x.EndDate.ToUnixTimeSeconds()));
        CreateMap<ScheduleMeetingCommand, Meeting>()
            .ForMember(dest => dest.StartDate, source => source.MapFrom(x => x.StartDate.ToUnixTimeSeconds()))
            .ForMember(dest => dest.EndDate, source => source.MapFrom(x => x.EndDate.ToUnixTimeSeconds()));
        CreateMap<MeetingSpeech, MeetingSpeechDto>();
        CreateMap<MeetingUserSetting, MeetingUserSettingDto>().ReverseMap();
        CreateMap<MeetingHistory, MeetingHistoryDto>();
        CreateMap<MeetingSpeakDetail, MeetingSpeakDetailDto>().ReverseMap();

        CreateMap<MeetingSummary, MeetingSummaryDto>().ReverseMap();
    }
}