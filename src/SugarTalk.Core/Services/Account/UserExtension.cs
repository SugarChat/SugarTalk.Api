using System.Linq;
using System.Security.Claims;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Messages;

namespace SugarTalk.Core.Services.Account
{
    public static class UserExtension
    {
        public static UserAccount ToUser(this ClaimsPrincipal principal)
        {
            var name = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var email = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var picture = principal.Claims.SingleOrDefault(x => x.Type == SugarTalkConstants.Picture)?.Value;
            var thirdPartyId = principal.Claims.Single(x => x.Type == SugarTalkConstants.ThirdPartyId).Value;
            // var thirdPartyFrom = principal.Claims.Single(x => x.Type == SugarTalkConstants.ThirdPartyFrom).Value;
            
            return new UserAccount
            {
                Email = email,
                Picture = picture,
                UserName = name,
                ThirdPartyUserId = thirdPartyId,
                // ThirdPartyFrom = Enum.Parse<ThirdPartyFrom>(thirdPartyFrom)
            };
        }
    }
}