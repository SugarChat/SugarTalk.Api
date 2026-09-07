using System;

namespace SugarTalk.Messages.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SugarTalkAuthorizeAttribute : Attribute
{
    public SugarTalkAuthorizeAttribute()
    {
    }

    public SugarTalkAuthorizeAttribute(params string[] roles)
    {
        Roles = roles;
    }

    public SugarTalkAuthorizeAttribute(string[] roles, string[] permissions)
    {
        Roles = roles;
        Permissions = permissions;
    }

    public string[] Roles { get; set; }

    public string[] Permissions { get; set; }
}
