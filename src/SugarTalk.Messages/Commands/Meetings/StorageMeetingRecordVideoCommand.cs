﻿using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class StorageMeetingRecordVideoCommand : ICommand
{
    public Guid MeetingId { get; set; }

    public string EgressId { get; set; }
        
    public Guid MeetingRecordId { get; set; }
}
    
public class StorageMeetingRecordVideoResponse : SugarTalkResponse
{
}