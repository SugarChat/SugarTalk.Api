using System;
using System.ComponentModel;
using System.Reflection;

namespace SugarTalk.Messages.Extensions;

public static class EnumExtension
{
    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        
        if (name == null)
            return null;

        var field = type.GetField(name);
        
        if (field == null)
            return null;

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        
        return attribute?.Description ?? name;
    }
}