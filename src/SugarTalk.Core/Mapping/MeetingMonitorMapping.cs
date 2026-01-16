using AutoMapper;
using SugarTalk.Core.Domain.MeetingMonitor;
using SugarTalk.Messages.Dto.MeetingMonitor;

namespace SugarTalk.Core.Mapping;

public class MeetingMonitorMapping: Profile
{
    public MeetingMonitorMapping()
    {
        CreateMap<MeetingMonitorKnowledgeDto, MeetingMonitorKnowledge>().ReverseMap();
    }
}