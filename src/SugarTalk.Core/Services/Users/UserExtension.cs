using System.Linq;
using System.Security.Claims;
using SugarTalk.Core.Entities;

namespace SugarTalk.Core.Services.Users
{
    public static class UserExtension
    {
        public static User ToUser(this ClaimsPrincipal principal)
        {
            var name = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var picture = principal.Claims.SingleOrDefault(x => x.Type == SugarTalkClaimType.Picture)?.Value;
            var thirdPartyId = principal.Claims.Single(x => x.Type == SugarTalkClaimType.ThirdPartyId).Value;

            return new User
            {
                Picture = picture,
                DisplayName = name,
                ThirdPartyId = thirdPartyId
            };
        }
    }
}