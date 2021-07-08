using Microsoft.AspNetCore.Authentication;

namespace SugarTalk.Api.Middlewares.Authentication
{
    public class GoogleAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string Audience { get; set; }
        
        public string Issuer { get; set; }
    }
}