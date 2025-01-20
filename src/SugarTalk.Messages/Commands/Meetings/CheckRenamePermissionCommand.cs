using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class CheckRenamePermissionCommand : ICommand
{
    public int UserId { get; set; }
    
    public int TargetUserId { get; set; }
    
    public Guid MeetingId { get; set; }
}

public class CheckRenamePermissionResponse : SugarTalkResponse<CheckRenamePermissionResponseData>
{
}

public class CheckRenamePermissionResponseData
{
    public bool CanRename { get; set; }
}