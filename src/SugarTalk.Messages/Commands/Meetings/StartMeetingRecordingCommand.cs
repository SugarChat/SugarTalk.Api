using System;
using Mediator.Net.Contracts;
using Newtonsoft.Json;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class StartMeetingRecordingCommand : ICommand
{
    public Guid MeetingId { get; set; }

    [JsonIgnore]
    public Guid? MeetingRecordId { get; set; }

    [JsonIgnore] 
    public bool IsRestartRecord { get; set; } = false;
}

public class StartMeetingRecordingResponse : SugarTalkResponse
{
    public Guid MeetingRecordId { get; set; }
    
    public string EgressId { get; set; }
}