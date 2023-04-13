using System;
using System.Linq;
using System.Security.Claims;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Messages;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Services.Users
{
    public static class UserExtension
    {
        public static User ToUser(this ClaimsPrincipal principal)
        {
            var name = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var email = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var picture = principal.Claims.SingleOrDefault(x => x.Type == SugarTalkConstants.Picture)?.Value;
            var thirdPartyId = principal.Claims.Single(x => x.Type == SugarTalkConstants.ThirdPartyId).Value;
            var thirdPartyFrom = principal.Claims.Single(x => x.Type == SugarTalkConstants.ThirdPartyFrom).Value;
            
            return new User
            {
                Email = email,
                Picture = picture,
                DisplayName = name,
                ThirdPartyId = thirdPartyId,
                ThirdPartyFrom = Enum.Parse<ThirdPartyFrom>(thirdPartyFrom)
            };
        }
    }
}