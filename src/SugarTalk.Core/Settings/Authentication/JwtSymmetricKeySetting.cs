using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Authentication;

public class JwtSymmetricKeySetting : IConfigurationSetting<string>
{
    public JwtSymmetricKeySetting(IConfiguration configuration)
    {
        Value = configuration.GetValue<string>("Authentication:Jwt:SymmetricKey");
    }
    
    public string Value { get; set; }
}