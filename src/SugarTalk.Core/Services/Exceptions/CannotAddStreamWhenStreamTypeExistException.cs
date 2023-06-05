using System;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotAddStreamWhenStreamTypeExistException : Exception
{
    public CannotAddStreamWhenStreamTypeExistException(MeetingStreamType streamType) : base(
        $"{streamType} had been used, cannot be manipulated repeatedly")
    {
    }
}