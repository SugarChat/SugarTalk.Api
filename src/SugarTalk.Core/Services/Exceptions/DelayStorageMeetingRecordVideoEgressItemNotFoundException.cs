using System;

namespace SugarTalk.Core.Services.Exceptions;

public class DelayStorageMeetingRecordVideoEgressItemNotFoundException : Exception
{
    public DelayStorageMeetingRecordVideoEgressItemNotFoundException() : base("Delay Storage Meeting Record Video Egress Item not found.")
    {
    }
}