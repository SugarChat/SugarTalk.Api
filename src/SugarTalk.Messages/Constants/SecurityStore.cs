using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SugarTalk.Messages.Constants;

public static class SecurityStore
{
    public static class Roles
    {
        public const string Administrator = nameof(Administrator);
    }

    public static class Permissions
    {
        public const string CanGetMeetingRelatedData = nameof(CanGetMeetingRelatedData);

        private static List<string> _allPermissions;

        public static List<string> AllPermissions
        {
            get
            {
                if (_allPermissions != null) return _allPermissions;

                _allPermissions = typeof(Permissions)
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                    .Select(field => (string)field.GetValue(null))
                    .ToList();

                return _allPermissions;
            }
        }
    }
}
