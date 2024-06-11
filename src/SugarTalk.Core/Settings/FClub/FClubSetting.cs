using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.FClub;

public class FClubSetting : IConfigurationSetting
{
    public FClubSetting(IConfiguration configuration)
    {
        BaseUrl = configuration.GetValue<string>("FClub:BaseUrl");
    }

    public string BaseUrl { get; set; }
}