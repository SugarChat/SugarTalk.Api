using System.Collections.Generic;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Requests.Meetings;

public class GenerateMeetingRecordTemporaryUrlCommand : ICommand
{
    public List<string> Urls { get; set; }
}

public class GenerateMeetingRecordTemporaryUrlResponse : IResponse
{
    public List<string> Urls { get; set; }
}