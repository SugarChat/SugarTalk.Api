using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotCancelAppointmentMeetingStatusException : Exception
{
    public CannotCancelAppointmentMeetingStatusException() : base(
        "Cannot cancel meeting when it is not pending status.")
    {
    }
}