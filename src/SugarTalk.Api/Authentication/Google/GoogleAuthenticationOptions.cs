using Microsoft.AspNetCore.Authentication;

namespace SugarTalk.Api.Authentication.Google
{
    public class GoogleAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string Audience { get; set; }
        
        public string Issuer { get; set; }
    }
}