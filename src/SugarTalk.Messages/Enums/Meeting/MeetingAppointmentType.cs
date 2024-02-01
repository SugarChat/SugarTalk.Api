using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingAppointmentType
{
    [Description("预约会议")]
    Appointment = 0,

    [Description("快速会议")]
    Quick = 1
}