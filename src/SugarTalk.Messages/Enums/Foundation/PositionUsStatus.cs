using System.ComponentModel;

namespace Smarties.Messages.Enums.Foundation;

public enum PositionUsStatus
{
    [Description("在职")]
    Active = 1,

    [Description("重新入职")]
    ReHired = 2,

    [Description("离开")]
    Leave = 3,

    [Description("离职")]
    Terminated = 4,

    [Description("已删除")]
    Deleted = 5,

    [Description("入职中")]
    Hiring = 6,

    [Description("入职逾期")]
    HiringNoShow = 7
}