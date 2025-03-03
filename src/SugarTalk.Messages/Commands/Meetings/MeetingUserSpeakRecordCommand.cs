using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Commands.Meetings;

public class MeetingUserSpeakRecordCommand : ICommand
{
    public string MeetingNumber { get; set; }

    public object Record { get; set; }
}

public class MeetingUserSpeakRecordResponse : IResponse
{
}