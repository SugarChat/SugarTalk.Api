using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotJoinMeetingWhenMeetingClosedException : Exception
{
    public CannotJoinMeetingWhenMeetingClosedException() : base(
        "会议室未开启，请确认会议时间或联系会议创建人。")
    {
    }
}