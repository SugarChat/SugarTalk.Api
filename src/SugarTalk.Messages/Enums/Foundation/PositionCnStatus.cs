using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Foundation;

public enum PositionCnStatus
{
    [Description("在职")]
    Active = 1,

    [Description("停薪留职")]
    Leave = 2,

    [Description("实习期")]
    Internship = 3,

    [Description("离职")]
    Terminated = 0
}