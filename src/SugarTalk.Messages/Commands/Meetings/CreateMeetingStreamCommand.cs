using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class CreateMeetingStreamCommand : ICommand
{
    public string Name { get; set; }
    
    public string Description { get; set; }

    public MeetingStreamType Type { get; set; } = MeetingStreamType.Created;

    public MeetingStreamStatus Status { get; set; } = MeetingStreamStatus.LiveStream;

    public bool Publish { get; set; } = true;
}

public class CreateMeetingStreamResponse : SugarTalkResponse<CreateMeetingStreamDto>
{
}
