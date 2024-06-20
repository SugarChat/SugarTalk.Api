using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.FClub;

public class FClubSetting : IConfigurationSetting
{
    public FClubSetting(IConfiguration configuration)
    {
        BaseUrl = configuration.GetValue<string>("FClub:BaseUrl");
        ApiKey = configuration.GetValue<string>("FClub:ApiKey");
    }

    public string BaseUrl { get; set; }

    public string ApiKey { get; set; }
}