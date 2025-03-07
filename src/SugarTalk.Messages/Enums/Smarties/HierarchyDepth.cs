using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Smarties;

public enum HierarchyDepth
{
    [Description("部门")]
    Department,
    
    [Description("公司")]
    Company,
    
    [Description("组别")]
    Group
}