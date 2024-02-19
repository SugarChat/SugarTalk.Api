using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarTalk.Core.Services.Caching
{
    public static class CacheService
    {
        public static string PrefixUserAccountByUserName = "UserAccountByUserName_";

        public static string GenerateUserAccountByUserNameKey(string userName)
        {
            return $"{PrefixUserAccountByUserName}:{userName}";
        }
    }
}
