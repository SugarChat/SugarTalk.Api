using Microsoft.AspNetCore.Authentication;

namespace SugarTalk.Api.Authentication.Wiltechs;

public class WiltechsAuthenticationOptions : AuthenticationSchemeOptions
{
    public string Authority { get; set; }
}